using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace ParcelTrackingApp
{
    // 1. Опис даних варіанта типами record
    public record City(int Id, string Name, string Region);
    public record Parcel(string TrackingNumber, string Sender, decimal Weight, string Status, int DestinationCityId);

    class Program
    {
        // Спільні списки для зберігання завантажених даних
        private static List<Parcel> _allParcels = new();
        private static List<City> _allCities = new();

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool isRunning = true;

            while (isRunning)
            {
                Console.WriteLine("\n=========================================");
                Console.WriteLine(" МЕНЮ: ВІДСТЕЖЕННЯ ПОСИЛОК (Вар.4)");
                Console.WriteLine("=========================================");
                Console.WriteLine("1. Завантажити дані (Послідовно - await)");
                Console.WriteLine("2. Завантажити дані (Одночасно - Task.WhenAll)");
                Console.WriteLine("3. LINQ 1: Фільтрація, сортування та проєкція");
                Console.WriteLine("4. LINQ 2: Групування та агрегування (статуси)");
                Console.WriteLine("5. LINQ 3: Об'єднання (Join) посилок із містами");
                Console.WriteLine("6. LINQ 4: Перевірка умов (Any / All)");
                Console.WriteLine("7. Зберегти звіт у JSON (асинхронно)");
                Console.WriteLine("0. Вихід");
                Console.Write("Оберіть пункт: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await LoadDataSequentiallyAsync();
                        break;
                    case "2":
                        await LoadDataConcurrentlyAsync();
                        break;
                    case "3":
                        ExecuteLinq1();
                        break;
                    case "4":
                        ExecuteLinq2();
                        break;
                    case "5":
                        ExecuteLinq3();
                        break;
                    case "6":
                        ExecuteLinq4();
                        break;
                    case "7":
                        await SaveReportToJsonAsync();
                        break;
                    case "0":
                        isRunning = false;
                        break;
                    default:
                        Console.WriteLine(">> Невірний вибір. Спробуйте ще раз.");
                        break;
                }
            }
        }

        #region Асинхронні джерела даних (Імітація віддаленого сервера)

        static async Task<List<Parcel>> LoadParcelsHub1Async()
        {
            await Task.Delay(1500); // Затримка 1.5 сек
            return new List<Parcel>
            {
                new Parcel("UA1001", "Rozetka", 2.5m, "В дорозі", 1),
                new Parcel("UA1002", "Цитрус", 0.8m, "Доставлено", 2),
                new Parcel("UA1003", "Алло", 5.0m, "Сортувальний центр", 3),
                new Parcel("UA1004", "Comfy", 15.2m, "В дорозі", 1),
                new Parcel("UA1005", "Privat", 0.1m, "Втрачено", 4),
                new Parcel("UA1006", "Rozetka", 1.2m, "Доставлено", 2),
                new Parcel("UA1007", "Епіцентр", 22.0m, "Сортувальний центр", 1)
            };
        }

        static async Task<List<Parcel>> LoadParcelsHub2Async()
        {
            await Task.Delay(2000); // Затримка 2.0 сек
            return new List<Parcel>
            {
                new Parcel("UA2001", "MakeUp", 0.5m, "В дорозі", 2),
                new Parcel("UA2002", "Kasta", 1.1m, "Доставлено", 4),
                new Parcel("UA2003", "Rozetka", 3.3m, "В дорозі", 3),
                new Parcel("UA2004", "F.ua", 7.5m, "Сортувальний центр", 1),
                new Parcel("UA2005", "Епіцентр", 35.0m, "В дорозі", 2),
                new Parcel("UA2006", "Comfy", 12.0m, "Доставлено", 3),
                new Parcel("UA2007", "IKEA", 45.0m, "В дорозі", 1)
            };
        }

        static async Task<List<City>> LoadCitiesAsync()
        {
            await Task.Delay(1000); // Затримка 1.0 сек
            return new List<City>
            {
                new City(1, "Київ", "Київська"),
                new City(2, "Вінниця", "Вінницька"),
                new City(3, "Львів", "Львівська"),
                new City(4, "Одеса", "Одеська")
            };
        }

        #endregion

        #region Завантаження даних

        static async Task LoadDataSequentiallyAsync()
        {
            Console.WriteLine("\nЗавантаження даних по черзі...");
            var sw = Stopwatch.StartNew();

            var hub1 = await LoadParcelsHub1Async();
            var hub2 = await LoadParcelsHub2Async();
            var cities = await LoadCitiesAsync();

            sw.Stop();
            MergeData(hub1, hub2, cities);

            Console.WriteLine($">> Готово! Час послідовного завантаження: {sw.ElapsedMilliseconds} мс");
        }

        static async Task LoadDataConcurrentlyAsync()
        {
            Console.WriteLine("\nОдночасне завантаження даних...");
            var sw = Stopwatch.StartNew();

            var taskHub1 = LoadParcelsHub1Async();
            var taskHub2 = LoadParcelsHub2Async();
            var taskCities = LoadCitiesAsync();

            await Task.WhenAll(taskHub1, taskHub2, taskCities);

            sw.Stop();
            MergeData(taskHub1.Result, taskHub2.Result, taskCities.Result);

            Console.WriteLine($">> Готово! Час одночасного завантаження: {sw.ElapsedMilliseconds} мс");
        }

        static void MergeData(List<Parcel> hub1, List<Parcel> hub2, List<City> cities)
        {
            _allParcels = hub1.Concat(hub2).ToList();
            _allCities = cities;
            Console.WriteLine($"Завантажено посилок: {_allParcels.Count}, міст: {_allCities.Count}");
        }

        static bool EnsureDataLoaded()
        {
            if (!_allParcels.Any())
            {
                Console.WriteLine(">> Спочатку завантажте дані (Пункт 1 або 2)!");
                return false;
            }
            return true;
        }

        #endregion

        #region LINQ Запити

        static void ExecuteLinq1()
        {
            if (!EnsureDataLoaded()) return;
            Console.WriteLine("\n--- LINQ 1: Важкі посилки в дорозі (Where, OrderBy, ThenBy, Select) ---");

            var query = _allParcels
                .Where(p => p.Status == "В дорозі" && p.Weight >= 2.0m)
                .OrderByDescending(p => p.Weight)
                .ThenBy(p => p.Sender)
                .Select(p => new { Номер = p.TrackingNumber, Відправник = p.Sender, Вага = p.Weight });

            foreach (var item in query)
            {
                Console.WriteLine($"- {item.Номер} від '{item.Відправник}', вага: {item.Вага} кг");
            }
        }

        static void ExecuteLinq2()
        {
            if (!EnsureDataLoaded()) return;
            Console.WriteLine("\n--- LINQ 2: Статистика за статусами (GroupBy, Count, Sum, Average) ---");

            var query = _allParcels
                .GroupBy(p => p.Status)
                .Select(g => new
                {
                    Статус = g.Key,
                    Кількість = g.Count(),
                    ЗагальнаВага = g.Sum(p => p.Weight),
                    СередняВага = g.Average(p => p.Weight)
                });

            foreach (var stat in query)
            {
                Console.WriteLine($"Статус: {stat.Статус,-18} | К-сть: {stat.Кількість} | Вага: {stat.ЗагальнаВага:F1} кг (сер. {stat.СередняВага:F2} кг)");
            }
        }

        static void ExecuteLinq3()
        {
            if (!EnsureDataLoaded()) return;
            Console.WriteLine("\n--- LINQ 3: Маршрутні листи (Join) ---");

            var query = _allParcels
                .Join(_allCities,
                    p => p.DestinationCityId,
                    c => c.Id,
                    (p, c) => new { p.TrackingNumber, p.Sender, c.Name, c.Region });

            foreach (var item in query)
            {
                Console.WriteLine($"Посилка {item.TrackingNumber} ({item.Sender}) прямує до м. {item.Name} ({item.Region} обл.)");
            }
        }

        static void ExecuteLinq4()
        {
            if (!EnsureDataLoaded()) return;
            Console.WriteLine("\n--- LINQ 4: Перевірка умов (Any / All) ---");

            bool hasLostParcels = _allParcels.Any(p => p.Status == "Втрачено");
            bool allHaveWeight = _allParcels.All(p => p.Weight > 0);

            Console.WriteLine($"Чи є втрачені посилки? -> {(hasLostParcels ? "ТАК" : "НІ")}");
            Console.WriteLine($"Чи всі посилки мають вагу > 0? -> {(allHaveWeight ? "ТАК" : "НІ")}");
        }

        #endregion

        #region Збереження у JSON

        static async Task SaveReportToJsonAsync()
        {
            if (!EnsureDataLoaded()) return;

            Console.WriteLine("\nЗбереження звіту...");

            // Готуємо дані для звіту
            var reportData = _allParcels
                .Join(_allCities, p => p.DestinationCityId, c => c.Id, (p, c) => new
                {
                    Tracking = p.TrackingNumber,
                    SenderInfo = p.Sender,
                    Destination = c.Name,
                    CurrentStatus = p.Status
                })
                .ToList();

            // Налаштування для читабельного JSON з українською мовою (без \uXXXX екранування)
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
            };

            string jsonString = JsonSerializer.Serialize(reportData, options);
            string filePath = "ParcelReport.json";

            try
            {
                await File.WriteAllTextAsync(filePath, jsonString);
                Console.WriteLine($">> Звіт успішно збережено у файл: {Path.GetFullPath(filePath)}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[ПОМИЛКА] Не вдалося записати файл. Причина: {ex.Message}");
            }
        }

        #endregion
    }
}
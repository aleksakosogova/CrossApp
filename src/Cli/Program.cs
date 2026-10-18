using System;
using System.Collections.Generic;
using System.IO;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Sinks;

namespace Cli;

// Додаткове завдання 1: Реалізація ConsoleSink
public class ConsoleSink : INotificationSink
{
    public void Notify(string message) => Console.WriteLine(message);
}

// Додаткове завдання 1: Реалізація FileSink (записує повідомлення від Sink у файл)
public class FileSink : INotificationSink
{
    private readonly string _path;
    public FileSink(string path) => _path = path;
    public void Notify(string message) => File.AppendAllText(_path, message + Environment.NewLine);
}

class Program
{
    static void Main()
    {
        // Створення шляху до лог-файлу та папки
        var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

        // Ініціалізація сервісу. Передаємо ConsoleSink, але можна замінити на new FileSink(logPath)
        var service = new OrderFulfillmentService(new ConsoleSink());

        // Крок 3: Локальний метод підписки
        void OnOrderChanged(object? sender, OrderChangedEventArgs e)
        {
            var line = $"{e.At:HH:mm:ss} {e.Kind,-15} {e.OrderId} Зміна: {e.Delta,3} Залишок: {e.QuantityAfter}";
            Console.WriteLine(line);
            File.AppendAllText(logPath, line + Environment.NewLine);
        }

        // Підписка основного обробника
        service.Changed += OnOrderChanged;

        // Крок 4: Лямбда-лічильник (замикання)
        int changes = 0;
        service.Changed += delegate { changes++; };

        // Створення сутностей через їхні правильні конструктори/фабрики
        var product = Product.Create("PRD-001", "SKU-100", "Мишка", "шт", 50); 
        var order1 = new Order("ORD-001", "SKU-100", 10);
        var order2 = new Order("ORD-002", "SKU-100", 5);

        Console.WriteLine("=== Успішний сценарій ===");
        service.ValidateAndFulfill(product, order1);
        Console.WriteLine($"Подій отримано: {changes}"); 

        Console.WriteLine("\n=== Після відписки ===");
        // Відписка основного обробника
        service.Changed -= OnOrderChanged;
        service.ValidateAndFulfill(product, order2);
        Console.WriteLine($"Подій отримано: {changes}"); // Лямбда продовжує рахувати

        Console.WriteLine("\n=== Сценарій відмови ===");
        var invalidOrder = new Order("ORD-003", "SKU-100", 100);
        try
        {
            // Інваріант викине виняток, подія не підніметься
            service.ValidateAndFulfill(product, invalidOrder);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка бізнес-логіки: {ex.Message}");
        }

        Console.WriteLine("\n=== Використання Func ===");
        var ordersList = new List<Order> { order1, order2, invalidOrder };
        // Використання Func для фільтрації колекції
        var bigOrders = service.FindAll(ordersList, o => o.RequestedQuantity > 8);
        foreach(var o in bigOrders)
        {
            Console.WriteLine($"Велике замовлення: {o.OrderId} (Кількість: {o.RequestedQuantity})");
        }

        Console.WriteLine("\n=== Додаткове завдання 2: Використання Action ===");
        // Перевірка імпорту з викликом Action-делегата на порожніх рядках
        string[] importData = { "ORD-010", "", "ORD-011", "  " };
        service.ImportOrders(importData, error => Console.WriteLine($"Пропущено рядок при імпорті: {error}"));
    }
}
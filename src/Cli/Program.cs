using System;
using System.Collections.Generic;
using System.IO;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Sinks;

namespace Cli;

public class ConsoleSink : INotificationSink
{
    public void Notify(string message) => Console.WriteLine(message);
}

class Program
{
    static void Main()
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

        var service = new OrderFulfillmentService(new ConsoleSink());

        void OnOrderChanged(object? sender, OrderChangedEventArgs e)
        {
            var line = $"{e.At:HH:mm:ss} {e.Kind,-15} {e.OrderId} Зміна: {e.Delta,3} Залишок: {e.QuantityAfter}";
            Console.WriteLine(line);
            File.AppendAllText(logPath, line + Environment.NewLine);
        }

        service.Changed += OnOrderChanged;

        int changes = 0;
        service.Changed += delegate { changes++; };

        // Використовуємо твій фабричний метод Create замість new Product
        // Передаємо 5 параметрів: id, sku, name, unit, quantity
        var product = Product.Create("PRD-001", "SKU-100", "Мишка", "шт", 50); 
        
        // Створюємо замовлення
        var order1 = new Order("ORD-001", "SKU-100", 10);
        var order2 = new Order("ORD-002", "SKU-100", 5);

        Console.WriteLine("=== Успішний сценарій ===");
        service.ValidateAndFulfill(product, order1);
        Console.WriteLine($"Подій отримано: {changes}"); 

        Console.WriteLine("\n=== Після відписки ===");
        service.Changed -= OnOrderChanged;
        service.ValidateAndFulfill(product, order2);
        Console.WriteLine($"Подій отримано: {changes}"); 

        Console.WriteLine("\n=== Сценарій відмови ===");
        var invalidOrder = new Order("ORD-003", "SKU-100", 100);
        try
        {
            service.ValidateAndFulfill(product, invalidOrder);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка бізнес-логіки: {ex.Message}");
        }

        Console.WriteLine("\n=== Використання Func ===");
        var ordersList = new List<Order> { order1, order2, invalidOrder };
        var bigOrders = service.FindAll(ordersList, o => o.RequestedQuantity > 8);
        foreach(var o in bigOrders)
        {
            Console.WriteLine($"Велике замовлення: {o.OrderId} (Кількість: {o.RequestedQuantity})");
        }
    }
}
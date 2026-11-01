using Core;
using Core.Reports;
using Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

// Отримуємо товари з SampleData
var items = SampleData.GetProducts();

var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

switch (command)
{
    case "report-top":
        Print(ReportService.TopByQuantity(items, 5));
        break;

    case "report-low":
        Print(ReportService.LowStock(items, 10));
        break;

    case "report-groups":
        foreach (var g in ReportService.ByUnit(items))
        {
            Console.WriteLine($"Одиниця: {g.Key,-6} | Позицій: {g.Count,3} | Сумарно: {g.TotalQuantity,6}");
        }
        break;

    case "report-sum":
        var t = ReportService.Totals(items);
        Console.WriteLine($"Всього позицій: {t.Items}, сумарно: {t.Total}, середнє: {t.Average:F2}, макс: {t.Max}");
        break;

    case "demo-deferred":
        RunDeferredExecutionDemo();
        break;

    default:
        Console.WriteLine("Доступні команди: report-top | report-low | report-groups | report-sum | demo-deferred");
        break;
}

static void Print(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
    {
        Console.WriteLine($"{l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}");
    }
}

static void RunDeferredExecutionDemo()
{
    var source = SampleData.GetProducts().ToList();
    var query = source.Where(p => p.Quantity < 10); // Запит побудовано, але ще НЕ виконано

    Console.WriteLine($"Дефіцитних до змін: {query.Count()}");
    
    // Створюємо новий продукт через публічний фабричний метод Create замість new Product
    source.Add(Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-999", "Скоба", "шт", 3)); 
    
    Console.WriteLine($"Дефіцитних після змін: {query.Count()}"); // Число змінилося завдяки відкладеному виконанню
}
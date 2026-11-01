using Core.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Core.Reports;

public static class ReportService
{
    // 1) Фільтр + сортування + проєкція
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold) =>
        source.Where(p => p.Quantity < threshold)
              .OrderBy(p => p.Quantity)
              .ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // 2) Топ- позицій зі стабільним порядком (ThenBy)
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) =>
        source.OrderByDescending(p => p.Quantity)
              .ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Take(n)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // 3) Групування з підрахунком і сумою
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source) =>
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
              .OrderByDescending(s => s.TotalQuantity)
              .ToList();

    // 4) Агрегати з обробкою порожньої послідовності
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, 
               list.Sum(p => p.Quantity), 
               list.Average(p => p.Quantity), 
               list.Max(p => p.Quantity));
    }

    // ДОДАТКОВЕ ЗАВДАННЯ 1: GroupJoin
    public static IReadOnlyList<ProductMovementDto> ProductsWithMovements(
        IEnumerable<Product> products, IEnumerable<StockMovement> movements) =>
        products.GroupJoin(
            movements,
            p => p.Sku,
            m => m.Sku,
            (p, mGroup) => new ProductMovementDto(p.Sku, p.Name, mGroup.Sum(m => m.ChangedQuantity))
        ).ToList();

    // ДОДАТКОВЕ ЗАВДАННЯ 3: Query Syntax
    public static IReadOnlyList<GroupSummary> ByUnitQuerySyntax(IEnumerable<Product> source) =>
        (from p in source
         group p by p.Unit into g
         orderby g.Sum(x => x.Quantity) descending
         select new GroupSummary(g.Key, g.Count(), g.Sum(x => x.Quantity)))
        .ToList();

    // ДОДАТКОВЕ ЗАВДАННЯ 4: Stopwatch
    public static string MeasurePerformance(int count)
    {
        var randomProducts = Enumerable.Range(1, count)
            .Select(i => Product.Create(i.ToString(), $"SKU-{i:00000}", $"Товар {i}", "шт", i % 50))
            .ToList();

        var sw = Stopwatch.StartNew();
        var _ = randomProducts.Where(p => p.Quantity < 10).ToList();
        sw.Stop();
        var timeWithoutIntermediate = sw.ElapsedMilliseconds;

        sw.Restart();
        var _2 = randomProducts.Where(p => p.Quantity < 10).ToList().Where(p => p.Name.Contains("1")).ToList();
        sw.Stop();
        var timeWithIntermediate = sw.ElapsedMilliseconds;

        return $"Без проміжного ToList: {timeWithoutIntermediate}ms, З проміжним ToList: {timeWithIntermediate}ms";
    }
}

public sealed record ProductMovementDto(string Sku, string Name, int TotalMoved);
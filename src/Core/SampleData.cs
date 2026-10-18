namespace Core;
using Core.Domain;
using System;
using System.Collections.Generic;

public static class SampleData
{
    public static IEnumerable<Product> Products()
    {
        return new List<Product>
        {
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-001", "Монітор 24", "шт", 10),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-002", "Клавіатура механічна", "шт", 25),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-003", "Миша бездротова", "шт", 40),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-004", "Килимок для миші", "шт", 100),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-005", "Кабель HDMI 2m", "шт", 50)
        };
    }
}
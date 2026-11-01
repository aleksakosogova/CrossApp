using System;
using System.Collections.Generic;
using Core.Domain;

namespace Core;

public static class SampleData
{
    public static List<Product> GetProducts()
    {
        return new List<Product>
        {
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-001", "Монітор 24", "шт", 10),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-002", "Клавіатура механічна", "шт", 25),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-003", "Миша бездротова", "шт", 40),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-004", "Килимок для миші", "шт", 100),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-005", "Кабель HDMI 2m", "шт", 50),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-006", "Термопаста MX-4", "шт", 4),        // Дефіцит (<10)
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-007", "Набір викруток для ПК", "компл", 8), // Дефіцит
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-008", "Навушники ігрові", "шт", 15),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-009", "Вебкамера 1080p", "шт", 12),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-010", "USB-хаб 4-port", "шт", 30),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-011", "Флешка USB 64GB", "шт", 45),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-012", "Зовнішній SSD 512GB", "шт", 6),   // Дефіцит (<10)
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-013", "Оперативна пам'ять 16GB", "шт", 20),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-014", "Блок живлення 600W", "шт", 14),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-015", "Очищувач для екранів", "наб", 25),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-016", "Перехідник Type-C", "шт", 18),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-017", "Підставка під ноутбук", "шт", 11),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-018", "Роутер Wi-Fi 6", "шт", 7)         // Дефіцит (<10)
        };
    }
}
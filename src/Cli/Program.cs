using Core.Domain;
using Core.Import;
using Core.Dto;
using Core.Services;

Console.WriteLine("=== Сценарій 1: успіх, зміна статусу та сервіс двох сутностей ===");
Product product = Product.Create("P-001", "sku-001", "Цемент М400 25кг", "шт", 100);
Console.WriteLine(product);

product.RegisterArrival(50);
product.ChangeStatus(ProductBatchStatus.Approved);
Console.WriteLine(product);

// Перевірка інваріанту на дві сутності через сервіс
Order order = new Order("ORD-555", "SKU-001", 30);
OrderFulfillmentService.ValidateAndFulfill(product, order);
Console.WriteLine($"Після виконання замовлення: {product}");

Console.WriteLine();
Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");
TryDo("недопустимий перехід статусу", () => product.ChangeStatus(ProductBatchStatus.Draft));
TryDo("замовлення більше за залишок (дві сутності)", () => OrderFulfillmentService.ValidateAndFulfill(product, new Order("ORD-666", "SKU-001", 5000)));
TryDo("порожній SKU", () => Product.Create("P-002", "", "Пісок", "т", 10));

Console.WriteLine();
Console.WriteLine("=== Сценарій 3: тестування імпорту ===");
var mockImportResult = new ImportResult<ProductDto>(
    new List<ProductDto>
    {
        new("P-100", "SKU-100", "Грунт", "міш", 50),
        new("P-101", "", "Помилковий", "шт", 10)
    },
    new List<string>()
);

var (successList, failedList) = ProductDomainImporter.Import(mockImportResult);
Console.WriteLine($"Успішно імпортовано: {successList.Count}");
Console.WriteLine($"Відхилено через інваріанти: {failedList.Count}");

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"[ПОРУШЕННЯ] {title}: виняток НЕ спрацював!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{title}: {ex.GetType().Name} - {ex.Message}");
    }
}
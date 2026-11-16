namespace Infrastructure.Persistence;

public sealed class ProductRow
{
    public string Id { get; set; } = "";
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "";
    public int Quantity { get; set; }
    public string Description { get; set; } = ""; // Поле для другої міграції (Додаткове завдання 1)
    public List<BatchRow> Batches { get; set; } = [];
}
using Core.Dto;

namespace Core.Domain;

public sealed class Product
{
    private int _quantity;

    public string Id { get; }
    public string Sku { get; }
    public string Name { get; }
    public string Unit { get; }
    public int Quantity => _quantity;
    public ProductBatchStatus Status { get; private set; } = ProductBatchStatus.Draft;

    private Product(string id, string sku, string name, string unit, int quantity)
    {
        Id = id;
        Sku = sku;
        Name = name;
        Unit = unit;
        _quantity = quantity;
    }

    // Фабричний метод із перевіркою всіх інваріантів при створенні
    public static Product Create(string id, string sku, string name, string unit, int quantity)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор обов'язковий", nameof(id));
        
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU не може бути порожнім", nameof(sku));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва не може бути порожньою", nameof(name));
        
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Початковий залишок не може бути від'ємним");

        return new Product(id.Trim(), sku.Trim().ToUpperInvariant(), name.Trim(), unit.Trim(), quantity);
    }

    // Метод приходу товару
    public void RegisterArrival(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Кількість приходу має бути більшою за нуль");

        _quantity += amount;
    }

    // Метод видачі товару (перевірка стану)
    public void Issue(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Кількість видачі має бути більшою за нуль");

        if (amount > _quantity)
            throw new InvalidOperationException($"Не можна видати {amount}: залишок {Sku} = {_quantity}");

        _quantity -= amount;
    }

    // Зміна статусу партії з перевіркою переходів через switch-expression
    public void ChangeStatus(ProductBatchStatus newStatus)
    {
        bool isValidTransition = (Status, newStatus) switch
        {
            (ProductBatchStatus.Draft, ProductBatchStatus.Approved) => true,
            (ProductBatchStatus.Approved, ProductBatchStatus.Archived) => true,
            (ProductBatchStatus.Draft, ProductBatchStatus.Archived) => true,
            _ => false
        };

        if (!isValidTransition)
            throw new InvalidOperationException($"Неможливо змінити статус зі стану '{Status}' на '{newStatus}'.");

        Status = newStatus;
    }

    // Мапінг у формат DTO і назад через фабрику для збереження перевірок
    public ProductDto ToDto() => new(Id, Sku, Name, Unit, Quantity);

    public static Product FromDto(ProductDto dto) =>
        Create(dto.Id, dto.Sku, dto.Name, dto.Unit, dto.Quantity);

    public override string ToString() => $"{Id} [{Sku}] {Name} - {Quantity} {Unit} [Статус: {Status}]";
}
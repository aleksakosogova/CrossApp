using Core.Domain;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class FileCatalogStoreTempTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _filePath;

    public FileCatalogStoreTempTests()
    {
        // Додаткове завдання 4: Створення тимчасового каталогу
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _filePath = Path.Combine(_tempDirectory, "test_items.json");
    }

    [Fact]
    public void Store_Should_Persist_And_Reload_Product_Correctly()
    {
        // Arrange
        var store1 = new FileCatalogStore(_filePath);
        var product = Product.Create("id-1", "SKU-001", "Тестовий товар", "шт", 15);
        
        // Act
        store1.Add(product);

        // Новий екземпляр сховища для перевірки зчитування з дисків
        var store2 = new FileCatalogStore(_filePath);
        var loadedList = store2.List();

        // Assert
        var loadedProduct = Assert.Single(loadedList);
        Assert.Equal(product.Id, loadedProduct.Id);
        Assert.Equal(product.Sku, loadedProduct.Sku);
        Assert.Equal(product.Name, loadedProduct.Name);
        Assert.Equal(product.Quantity, loadedProduct.Quantity);
    }

    public void Dispose()
    {
        // Додаткове завдання 4: Очищення тимчасового каталогу після завершення тесту
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
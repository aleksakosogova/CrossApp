using Core.Domain;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Core.Tests;

public class EfCatalogStoreMemoryTests
{
    [Fact]
    public async Task EfCatalogStore_Memory_Works()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var store = new EfCatalogStore(context);
        var product = Product.Create("id-1", "SKU-MEM", "Тест Пам'ять", "шт", 10);

        await store.AddAsync(product);
        var fetched = await store.GetByIdAsync("id-1");

        Assert.NotNull(fetched);
        Assert.Equal("SKU-MEM", fetched.Sku);
    }
}
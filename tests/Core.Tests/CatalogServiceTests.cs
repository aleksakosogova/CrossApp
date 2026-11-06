using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;
using Xunit;

public class CatalogServiceTests
{
    [Fact]
    public void Issue_WithExcessiveQuantity_Throws_AndKeepsState()
    {
        var product = Product.Create("id-1", "SKU-1", "Кабель", "м", 10);
        
        Assert.Throws<InvalidOperationException>(() => product.Issue(15));
        Assert.Equal(10, product.Quantity);
    }

    [Fact]
    public void Create_WithEmptySku_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Product.Create("id-1", "", "Назва", "шт", 10));
    }

    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        
        var importer = new JsonCatalogImporter();
        var tempFile = Path.GetTempFileName();
        
        try
        {
            await File.WriteAllTextAsync(tempFile, "[]");
            
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await importer.LoadAsync(tempFile, cts.Token));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ImportAsync_WithInvalidData_ThrowsDomainException()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "[{\"Id\": \"1\", \"Sku\": \"\", \"Name\": \"Test\", \"Unit\": \"шт\", \"Quantity\": 5}]");
            var importer = new JsonCatalogImporter();
            
            await Assert.ThrowsAsync<DomainException>(
                async () => await importer.LoadAsync(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ImportAsync_WithLinkedTokenAndTimeout_TriggersTimeout()
    {
        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(5));
        
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

        await Task.Delay(30);

        var service = new CatalogService(new InMemoryCatalogStore());
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "[]");

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await service.ImportAsync(new JsonCatalogImporter(), tempFile, null, linkedCts.Token));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}

internal sealed class JsonAccountImporterStub : ICatalogImporter
{
    public async Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default)
    {
        await Task.Delay(100, ct);
        return Array.Empty<Product>();
    }

    public async IAsyncEnumerable<Product> LoadStreamAsync(string path, [EnumeratorCancellation] CancellationToken ct = default)
    {
        await Task.Delay(100, ct);
        yield break;
    }
}
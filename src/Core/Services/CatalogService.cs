using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class CatalogService
{
    private readonly ICatalogStore _store;

    public event EventHandler<CatalogChangedEventArgs>? Changed;

    public CatalogService(ICatalogStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public Product Add(string sku, string name, string unit, int quantity)
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var product = Product.Create(id, sku, name, unit, quantity);
        _store.Add(product);
        OnChanged(product, CatalogChangeKind.Added, product.Quantity);
        return product;
    }

    public Product Receive(string id, int amount)
    {
        var product = _store.GetById(id) ?? throw new InvalidOperationException($"Товар із id '{id}' не знайдено.");
        product.RegisterArrival(amount);
        OnChanged(product, CatalogChangeKind.Received, amount);
        return product;
    }

    public Product Issue(string id, int amount)
    {
        var product = _store.GetById(id) ?? throw new InvalidOperationException($"Товар із id '{id}' не знайдено.");
        product.Issue(amount);
        OnChanged(product, CatalogChangeKind.Issued, amount);
        return product;
    }

    // Базовий асинхронний імпорт із підтримкою IProgress<int> (Додаткове завдання 3)
    public async Task<int> ImportAsync(ICatalogImporter importer, string path, IProgress<int>? progress = null, CancellationToken ct = default)
    {
        if (importer == null) throw new ArgumentNullException(nameof(importer));
        
        var items = await importer.LoadAsync(path, ct);
        var total = items.Count;
        var added = 0;

        for (int i = 0; i < total; i++)
        {
            ct.ThrowIfCancellationRequested();
            var item = items[i];

            if (_store.GetById(item.Id) is not null) 
            {
                progress?.Report((i + 1) * 100 / total);
                continue;
            }

            _store.Add(item);
            OnChanged(item, CatalogChangeKind.Added, item.Quantity);
            added++;

            progress?.Report((i + 1) * 100 / total);
        }

        return added;
    }

    // Додаткове завдання 1: Потоковий імпорт через await foreach з IProgress
    public async Task<int> ImportStreamAsync(ICatalogImporter importer, string path, IProgress<int>? progress = null, CancellationToken ct = default)
    {
        if (importer == null) throw new ArgumentNullException(nameof(importer));

        var added = 0;
        int processed = 0;

        await foreach (var item in importer.LoadStreamAsync(path, ct))
        {
            processed++;
            ct.ThrowIfCancellationRequested();

            if (_store.GetById(item.Id) is not null) 
            {
                progress?.Report(processed);
                continue;
            }

            _store.Add(item);
            OnChanged(item, CatalogChangeKind.Added, item.Quantity);
            added++;

            progress?.Report(processed);
        }

        return added;
    }

    private void OnChanged(Product product, CatalogChangeKind kind, int amount)
    {
        Changed?.Invoke(this, new CatalogChangedEventArgs(product, kind, amount));
    }
}

public enum CatalogChangeKind
{
    Added,
    Received,
    Issued
}

public sealed class CatalogChangedEventArgs : EventArgs
{
    public Product Product { get; }
    public CatalogChangeKind Kind { get; }
    public int Amount { get; }

    public CatalogChangedEventArgs(Product product, CatalogChangeKind kind, int amount)
    {
        Product = product;
        Kind = kind;
        Amount = amount;
    }
}
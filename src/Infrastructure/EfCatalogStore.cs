using Core.Abstractions;
using Core.Domain;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public sealed class EfCatalogStore(AppDbContext db) : IAsyncCatalogStore
{
    private static Product ToDomain(ProductRow row) =>
        Product.Create(row.Id, row.Sku, row.Name, row.Unit, row.Quantity);

    private static ProductRow ToRow(Product p) => new()
    {
        Id = p.Id,
        Sku = p.Sku,
        Name = p.Name,
        Unit = p.Unit,
        Quantity = p.Quantity
    };

    public async Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken = default)
    {
        var rows = await db.Products
            .AsNoTracking()
            .OrderBy(x => x.Sku)
            .ToListAsync(cancellationToken);

        return rows.Select(ToDomain).ToList();
    }

    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var row = await db.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return row is not null ? ToDomain(row) : null;
    }

    public async Task AddAsync(Product item, CancellationToken cancellationToken = default)
    {
        db.Products.Add(ToRow(item));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Product item, CancellationToken cancellationToken = default)
    {
        var row = await db.Products.SingleAsync(x => x.Id == item.Id, cancellationToken);
        row.Sku = item.Sku;
        row.Name = item.Name;
        row.Unit = item.Unit;
        row.Quantity = item.Quantity;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RemoveAsync(string id, CancellationToken cancellationToken = default)
    {
        var row = await db.Products.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (row is null) return false;

        db.Products.Remove(row);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
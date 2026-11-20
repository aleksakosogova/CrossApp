using Core.Domain;

namespace Core.Abstractions;

public interface IAsyncCatalogStore
{
    Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task AddAsync(Product item, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product item, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string id, CancellationToken cancellationToken = default);
}

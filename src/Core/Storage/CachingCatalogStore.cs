namespace Core.Storage;
using Core.Abstractions;
using Core.Domain;

public sealed class CachingCatalogStore(ICatalogStore innerStore) : ICatalogStore
{
    private readonly ICatalogStore _innerStore = innerStore ?? throw new ArgumentNullException(nameof(innerStore));
    private IReadOnlyList<Product>? _cachedList;

    public IReadOnlyList<Product> List() => _cachedList ??= _innerStore.List();
    public Product? GetById(string id) => _innerStore.GetById(id);
    public void Add(Product item) { _innerStore.Add(item); _cachedList = null; }
    public void Update(Product item) { _innerStore.Update(item); _cachedList = null; }
    public bool Remove(string id) { var result = _innerStore.Remove(id); if (result) _cachedList = null; return result; }
    public IReadOnlyList<Product> Find(Func<Product, bool> predicate) => _innerStore.Find(predicate);
}
using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public class FileCatalogStore : ICatalogStore
{
    private readonly string _path;
    private readonly Dictionary<string, Product> _cache = new();
    private bool _loaded;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public FileCatalogStore(string filePath)
    {
        _path = filePath;
    }

    private void Flush()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var tmp = _path + ".tmp";

        if (_path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            // Формат CSV (Додаткове завдання 3)
            var lines = new List<string> { "Id,Sku,Name,Unit,Quantity" };
            foreach (var p in _cache.Values)
            {
                lines.Add($"{p.Id},{p.Sku},{p.Name},{p.Unit},{p.Quantity}");
            }
            File.WriteAllLines(tmp, lines);
        }
        else
        {
            // Формат JSON
            var dtos = _cache.Values.Select(product => product.ToDto()).ToList();
            File.WriteAllText(tmp, JsonSerializer.Serialize(dtos, Options));
        }

        File.Move(tmp, _path, overwrite: true);
    }

    private void EnsureLoaded()
    {
        if (_loaded) return;
        if (!File.Exists(_path)) { _loaded = true; return; }
        
        _cache.Clear();

        if (_path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            var lines = File.ReadAllLines(_path);
            foreach (var line in lines.Skip(1)) // Пропускаємо заголовки
            {
                var parts = line.Split(',');
                if (parts.Length >= 5)
                {
                    if (int.TryParse(parts[4], out var qty))
                    {
                        var product = Product.Create(parts[0], parts[1], parts[2], parts[3], qty);
                        _cache[product.Id] = product;
                    }
                }
            }
        }
        else
        {
            var json = File.ReadAllText(_path);
            var dtos = JsonSerializer.Deserialize<List<ProductDto>>(json, Options) ?? [];
            foreach (var dto in dtos)
            {
                var product = Product.FromDto(dto);
                _cache[product.Id] = product;
            }
        }

        _loaded = true;
    }

    public IEnumerable<Product> GetAll()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public IReadOnlyList<Product> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Product? GetById(string id)
    {
        EnsureLoaded();
        return _cache.TryGetValue(id, out var product) ? product : null;
    }

    public void Add(Product product)
    {
        EnsureLoaded();
        _cache[product.Id] = product;
        Flush();
    }

    public void Update(Product product)
    {
        EnsureLoaded();
        if (_cache.ContainsKey(product.Id))
        {
            _cache[product.Id] = product;
            Flush();
        }
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        var removed = _cache.Remove(id);
        if (removed)
        {
            Flush();
        }
        return removed;
    }

    public IReadOnlyList<Product> Find(Func<Product, bool> predicate)
    {
        EnsureLoaded();
        return _cache.Values.Where(predicate).ToList();
    }
}
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ImportResult<ProductDto> Load(string path)
    {
        string json = File.ReadAllText(path);
        var items = JsonSerializer.Deserialize<List<ProductDto>>(json, SerializerOptions) ?? [];

        return new ImportResult<ProductDto>(items, []);
    }
}

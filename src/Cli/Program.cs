using System.Text.Json;
using Core.Domain;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var connectionString = "Data Source=data/app.db";
var csb = new SqliteConnectionStringBuilder(connectionString);
if (!Path.IsPathRooted(csb.DataSource))
{
    var full = Path.Combine(Directory.GetCurrentDirectory(), csb.DataSource);
    Directory.CreateDirectory(Path.GetDirectoryName(full)!);
    csb.DataSource = full;
}

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite(csb.ConnectionString)
    .Options;

using var dbContext = new AppDbContext(options);

// Автоматично застосовуємо міграції, щоб таблиці точно існували в базі
dbContext.Database.Migrate();

var catalogStore = new EfCatalogStore(dbContext);

var jsonPath = Path.Combine("data", "items.json");
if (!File.Exists(jsonPath))
{
    Console.WriteLine($"Файл імпорту не знайдено за шляхом: {jsonPath}");
    return;
}

Console.WriteLine("Виконується імпорт даних із JSON у базу даних SQLite...");

try
{
    var jsonString = File.ReadAllText(jsonPath);
    
    var dtos = JsonSerializer.Deserialize<List<ImportProductDto>>(jsonString, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (dtos != null)
    {
        int importedCount = 0;
        foreach (var dto in dtos)
        {
            var existing = await dbContext.Products.FirstOrDefaultAsync(p => p.Sku == dto.Sku);
            if (existing != null)
            {
                Console.WriteLine($"Товар із SKU '{dto.Sku}' вже існує в базі. Пропускаємо.");
                continue;
            }

            var product = Product.Create(dto.Sku, dto.Name, dto.Unit, dto.Description, dto.Quantity);
            await catalogStore.AddAsync(product);
            importedCount++;
        }
        Console.WriteLine($"Імпорт успішно завершено! Імпортовано нових товарів: {importedCount}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Помилка під час імпорту JSON: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Деталі внутрішньої помилки: {ex.InnerException.Message}");
    }
}

// DTO для десеріалізації
public class ImportProductDto
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
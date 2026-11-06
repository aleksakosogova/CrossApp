using System.Reflection;
using Core.Configuration;
using Core.Domain;
using Core.Storage;
using Microsoft.Extensions.Configuration;

var envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{envName}.json", optional: true)
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables(prefix: "CROSSAPP_")
    .AddCommandLine(args)
    .Build();

var options = config.Get<AppOptions>() ?? new AppOptions();

// Валідація налаштувань на старті (Додаткове завдання 1)
if (string.IsNullOrWhiteSpace(options.Storage.DataPath))
{
    Console.Error.WriteLine("Помилка конфігурації: шлях DataPath не може бути порожнім.");
    Environment.Exit(2);
}

if (string.IsNullOrWhiteSpace(options.Storage.FileName) || !options.Storage.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine("Помилка конфігурації: FileName має закінчуватися на .json.");
    Environment.Exit(2);
}

// Перевизначення через командний рядок або змінні середовища (Додаткове завдання 2)
var cliDataPath = config["data-path"] ?? config["Storage:DataPath"];
var legacy = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");

var finalDataPath = !string.IsNullOrWhiteSpace(cliDataPath) && cliDataPath != options.Storage.DataPath 
    ? cliDataPath 
    : (string.IsNullOrWhiteSpace(legacy) ? options.Storage.DataPath : legacy);

var storageOptions = new StorageOptions
{
    DataPath = finalDataPath,
    FileName = options.Storage.FileName
};

var fullPath = storageOptions.ResolveFullPath(AppContext.BaseDirectory);
var apiKey = config["Integration:ApiKey"];

Console.WriteLine($"Файл даних: {fullPath}");
Console.WriteLine(apiKey is null ? "ApiKey: не заданий" : $"ApiKey: присутній, довжина {apiKey.Length}");

var store = new FileCatalogStore(fullPath);

// Перевірка аргументу export для демонстрації виводу як у методичці
if (args.Contains("export") || args.Contains("-- export"))
{
    var items = store.List();
    Console.WriteLine($"Експортовано {items.Count} записи.");
}
else
{
    Console.WriteLine("Конфігурацію та сховище успішно ініціалізовано.");
}
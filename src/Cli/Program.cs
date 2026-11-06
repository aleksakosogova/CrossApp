using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Domain;
using Core.Services;
using Core.Storage;

// Ініціалізація зв'язаного токена для обробки Ctrl+C та опціонального тайм-ауту
using var userCts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) => 
{ 
    e.Cancel = true; 
    userCts.Cancel(); 
};

// Приклад зв'язаного токена з тайм-аутом (наприклад, 10 секунд на всю операцію)
using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

try
{
    var dataPath = args.Length > 0 ? args[0] : "data/sample.json";

    var store = new InMemoryCatalogStore();
    var service = new CatalogService(store);

    // Додаткове завдання 3: Підписник на прогрес (малює відсотки у консолі)
    var progress = new Progress<int>(percent =>
    {
        Console.Write($"\rПрогрес імпорту: {percent}% ");
    });

    Console.WriteLine("Початок асинхронного імпорту...");
    
    // Використовуємо потоковий або звичайний імпорт із прогресом та токеном
    var added = await service.ImportStreamAsync(new JsonCatalogImporter(), dataPath, progress, linkedCts.Token);
    
    Console.WriteLine($"\nІмпортовано записів: {added}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nІмпорт скасовано користувачем або за тайм-аутом.");
}
catch (DomainException ex)
{
    Console.WriteLine($"\nДані відхилено: {ex.Message}");
}
catch (FileNotFoundException)
{
    var dataPath = args.Length > 0 ? args[0] : "data/sample.json";
    Console.WriteLine($"\nФайл не знайдено: {dataPath}");
}
catch (Exception ex)
{
    Console.WriteLine($"\nСталася непередбачена технічна помилка: {ex.Message}");
}
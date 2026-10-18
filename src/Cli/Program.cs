using Cli;
using Core.Services;

var store = StoreFactory.Create(args);
var service = new CatalogService(store);
Console.WriteLine($"Сховище: {store.GetType().Name} (внутрішнє: {(args.Contains("--file") ? "FileCatalogStore" : "InMemoryCatalogStore")})");

try
{
    var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
    service.Receive(created.Id, 25);
    foreach (var p in service.All()) Console.WriteLine($" {p.Id} {p.Sku,-10} {p.Name,-22} {p.Quantity, 5} {p.Unit}");
    
    Console.WriteLine("\nСценарій відмови:");
    service.Receive("invalid-id-999", 10);
}
catch (InvalidOperationException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Помилка: {ex.Message}");
    Console.ResetColor();
}
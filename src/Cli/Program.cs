using System.Runtime.InteropServices;
using System.Text.Json;

// Налаштування для коректного відображення кирилиці в консолі Windows
Console.OutputEncoding = System.Text.Encoding.UTF8;

var info = new
{
    Title = "CrossApp - практикум з крос-платформного програмування",
    Student = "Косогова Олександра, група ФЕІ-37с",
    OSDescription = RuntimeInformation.OSDescription,
    OSEnvironment = Environment.OSVersion.ToString(),
    Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
    DotNetVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    AppDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Предметна область: Замовлення (клієнт, товар, замовлення, рядок замовлення)"
};

if (args.Contains("--json"))
{
    // Додаткове завдання: вивід у форматі JSON з підтримкою кирилиці
    var options = new JsonSerializerOptions 
    { 
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    Console.WriteLine(JsonSerializer.Serialize(info, options));
}
else
{
    // Стандартний вивід
    Console.WriteLine(info.Title);
    Console.WriteLine($"Студент: {info.Student}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"OC (OSDescription): {info.OSDescription}");
    Console.WriteLine($"OC (Environment)  : {info.OSEnvironment}");
    Console.WriteLine($"Архітектура процесу: {info.Architecture}");
    Console.WriteLine($"Версія .NET (CLR) : {info.DotNetVersion}");
    Console.WriteLine($"Runtime           : {info.Runtime}");
    Console.WriteLine($"Каталог застосунку: {info.AppDirectory}");
    Console.WriteLine($"Поточний каталог  : {info.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine(info.Domain);
}
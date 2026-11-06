namespace Core.Configuration;

public sealed class AppOptions
{
    public StorageOptions Storage { get; init; } = new();
    public string EnvironmentName { get; init; } = "Development";
}

public sealed class StorageOptions
{
    public string DataPath { get; init; } = "data";
    public string FileName { get; init; } = "items.json";
    public string Format { get; init; } = "json"; // Додаткове завдання 3: формат "json" або "csv"

    public string ResolveFullPath(string baseDir)
    {
        var root = Path.IsPathRooted(DataPath) ? DataPath : Path.Combine(baseDir, DataPath);
        Directory.CreateDirectory(root);
        // Якщо формат csv, автоматично змінюємо розширення файлу на .csv, якщо вказано items.json
        var fileName = Format.Equals("csv", StringComparison.OrdinalIgnoreCase) && FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(FileName, ".csv")
            : FileName;
        return Path.GetFullPath(Path.Combine(root, fileName));
    }
}
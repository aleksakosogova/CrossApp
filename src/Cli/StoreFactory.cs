namespace Cli;
using Core.Abstractions;
using Core.Storage;
using Core;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args)
    {
        bool useFile = args.Contains("--file");
        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
        ICatalogStore baseStore = useFile ? new FileCatalogStore(dataPath) : new InMemoryCatalogStore(SampleData.Products());
        return new CachingCatalogStore(baseStore);
    }
}
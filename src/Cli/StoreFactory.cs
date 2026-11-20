namespace Cli;

using Core;
using Core.Abstractions;
using Core.Storage;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args)
    {
        bool useFile = args.Contains("--file");
        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
        ICatalogStore baseStore = useFile ? new FileCatalogStore(dataPath) : new InMemoryCatalogStore(SampleData.GetProducts());
        return new CachingCatalogStore(baseStore);
    }
}

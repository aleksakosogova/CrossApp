using Core.Domain;
using Core.Dto;

namespace Core.Import;

public class ProductDomainImporter
{
    public static (List<Product> Products, List<(ProductDto Dto, string Error)> Failed) Import(ImportResult<ProductDto> importResult)
    {
        var products = new List<Product>();
        var failed = new List<(ProductDto Dto, string Error)>();

        foreach (var dto in importResult.Items)
        {
            try
            {
                var product = Product.FromDto(dto);
                products.Add(product);
            }
            catch (Exception ex)
            {
                failed.Add((dto, ex.Message));
            }
        }

        return (products, failed);
    }
}
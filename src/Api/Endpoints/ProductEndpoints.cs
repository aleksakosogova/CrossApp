using System.Security.Cryptography;
using Contracts;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        // GET з пошуком, сортуванням і пагінацією на боці API (асинхронно)
        group.MapGet("/", async (
            int? page,
            int? pageSize,
            string? search,
            string? sortBy,
            string? sortDir,
            IAsyncCatalogStore store,
            CancellationToken cancellationToken) =>
        {
            var products = await store.ListAsync(cancellationToken);
            var query = products.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Sku.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Unit.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy?.ToLowerInvariant()) switch
            {
                "name" => desc ? query.OrderByDescending(p => p.Name, StringComparer.OrdinalIgnoreCase) : query.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
                "quantity" => desc ? query.OrderByDescending(p => p.Quantity) : query.OrderBy(p => p.Quantity),
                "unit" => desc ? query.OrderByDescending(p => p.Unit, StringComparer.OrdinalIgnoreCase) : query.OrderBy(p => p.Unit, StringComparer.OrdinalIgnoreCase),
                _ => desc ? query.OrderByDescending(p => p.Sku, StringComparer.OrdinalIgnoreCase) : query.OrderBy(p => p.Sku, StringComparer.OrdinalIgnoreCase)
            };

            var total = query.Count();
            int pageNumber = Math.Max(1, page ?? 1);
            int size = Math.Clamp(pageSize ?? 50, 1, 200);

            var items = query
                .Skip((pageNumber - 1) * size)
                .Take(size)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Sku = p.Sku,
                    Name = p.Name,
                    Unit = p.Unit,
                    Quantity = p.Quantity
                })
                .ToList();

            return TypedResults.Ok(new PagedProductsResult(items, total, pageNumber, size));
        });

        // GET за ID (асинхронно)
        group.MapGet("/{id}", async Task<Results<Ok<ProductDto>, NotFound>> (string id, IAsyncCatalogStore store, CancellationToken cancellationToken) =>
        {
            var found = await store.GetByIdAsync(id, cancellationToken);
            if (found is null)
                return TypedResults.NotFound();

            return TypedResults.Ok(new ProductDto
            {
                Id = found.Id,
                Sku = found.Sku,
                Name = found.Name,
                Unit = found.Unit,
                Quantity = found.Quantity
            });
        });

        // POST (асинхронно)
        group.MapPost("/", async Task<Results<Created<ProductDto>, BadRequest<string>>> (SaveProductRequest request, IAsyncCatalogStore store, CancellationToken cancellationToken) =>
        {
            try
            {
                var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
                var created = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity);
                await store.AddAsync(created, cancellationToken);

                var dto = new ProductDto
                {
                    Id = created.Id,
                    Sku = created.Sku,
                    Name = created.Name,
                    Unit = created.Unit,
                    Quantity = created.Quantity
                };

                return TypedResults.Created($"/api/products/{created.Id}", dto);
            }
            catch (ArgumentException ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        });

        // PUT (асинхронно)
        group.MapPut("/{id}", async Task<Results<Ok<ProductDto>, NotFound, BadRequest<string>>> (string id, SaveProductRequest request, IAsyncCatalogStore store, CancellationToken cancellationToken) =>
        {
            var existing = await store.GetByIdAsync(id, cancellationToken);
            if (existing is null)
                return TypedResults.NotFound();

            try
            {
                var replacement = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity);
                await store.UpdateAsync(replacement, cancellationToken);

                var dto = new ProductDto
                {
                    Id = replacement.Id,
                    Sku = replacement.Sku,
                    Name = replacement.Name,
                    Unit = replacement.Unit,
                    Quantity = replacement.Quantity
                };

                return TypedResults.Ok(dto);
            }
            catch (ArgumentException ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        });

        // DELETE (асинхронно)
        group.MapDelete("/{id}", async Task<Results<NoContent, NotFound>> (string id, IAsyncCatalogStore store, CancellationToken cancellationToken) =>
        {
            var success = await store.RemoveAsync(id, cancellationToken);
            return success
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });
    }
}
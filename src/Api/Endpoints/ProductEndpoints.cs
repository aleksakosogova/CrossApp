using System.Security.Cryptography;
using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        // GET зі структурою пагінації та пошуку (асинхронно)
        group.MapGet("/", async (int? page, int? pageSize, string? search, IAsyncCatalogStore store, CancellationToken cancellationToken) =>
        {
            var products = await store.ListAsync(cancellationToken);
            var query = products.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                         p.Sku.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var total = query.Count();
            int pageNumber = page ?? 1;
            int size = pageSize ?? 20;

            var items = query
                .Skip((pageNumber - 1) * size)
                .Take(size)
                .Select(p => p.ToDto())
                .ToList();

            return TypedResults.Ok(new { items, total, page = pageNumber, pageSize = size });
        });

        // GET за ID (асинхронно)
        group.MapGet("/{id}", async Task<Results<Ok<ProductDto>, NotFound>> (string id, IAsyncCatalogStore store, CancellationToken cancellationToken) =>
        {
            var found = await store.GetByIdAsync(id, cancellationToken);
            return found is not null
                ? TypedResults.Ok(found.ToDto())
                : TypedResults.NotFound();
        });

        // POST (асинхронно) з коректним TypedResults.Created для заголовка Location
        group.MapPost("/", async Task<Results<Created<ProductDto>, BadRequest<string>>> (SaveProductRequest request, IAsyncCatalogStore store, CancellationToken cancellationToken) =>
        {
            try
            {
                var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
                var created = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity);
                await store.AddAsync(created, cancellationToken);

                return TypedResults.Created($"/api/products/{created.Id}", created.ToDto());
            }
            catch (ArgumentException ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        });

        // PUT (асинхронно)
        group.MapPut("/{id}", async Task<Results<Ok<ProductDto>, NotFound>> (string id, SaveProductRequest request, IAsyncCatalogStore store, CancellationToken cancellationToken) =>
        {
            var existing = await store.GetByIdAsync(id, cancellationToken);
            if (existing is null)
                return TypedResults.NotFound();

            var replacement = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity);
            await store.UpdateAsync(replacement, cancellationToken);

            return TypedResults.Ok(replacement.ToDto());
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
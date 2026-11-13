using System.Security.Cryptography;
using Api.Contracts;
using Api.Filters;
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

        // GET зі структурою пагінації та пошуку
        group.MapGet("/", (int? page, int? pageSize, string? search, ICatalogStore store) =>
        {
            var query = store.List().AsEnumerable();

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

        // GET за ID
        group.MapGet("/{id}", Results<Ok<ProductDto>, NotFound> (string id, ICatalogStore store) =>
            store.GetById(id) is { } found
                ? TypedResults.Ok(found.ToDto())
                : TypedResults.NotFound());

        // POST через FluentValidation Filter
        group.MapPost("/", (SaveProductRequest request, ICatalogStore store) =>
        {
            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var created = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity);
            store.Add(created);

            return Results.Created($"/api/products/{created.Id}", created.ToDto());
        }).AddEndpointFilter<ValidationFilter<SaveProductRequest>>();

        // PUT через FluentValidation Filter
        group.MapPut("/{id}", Results<Ok<ProductDto>, NotFound> (string id, SaveProductRequest request, ICatalogStore store) =>
        {
            if (store.GetById(id) is null)
                return TypedResults.NotFound();

            var replacement = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity);
            store.Update(replacement);

            return TypedResults.Ok(replacement.ToDto());
        }).AddEndpointFilter<ValidationFilter<SaveProductRequest>>();

        // DELETE
        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, ICatalogStore store) =>
            store.Remove(id)
                ? TypedResults.NoContent()
                : TypedResults.NotFound());
    }
}
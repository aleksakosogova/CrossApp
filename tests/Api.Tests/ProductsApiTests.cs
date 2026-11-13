using System.Net;
using System.Net.Http.Json;
using Api.Contracts;
using Core.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Api.Tests;

public class ProductsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_And_Get_Product_Flow_Works()
    {
        // Arrange
        var request = new SaveProductRequest("TEST-SKU", "Тестовий товар", "шт", 15);

        // Act - POST (Створення)
        var postResponse = await _client.PostAsJsonAsync("/api/products", request);

        // Assert - перевірка статусу 201 Created та заголовка Location
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);

        var createdProduct = await postResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(createdProduct);
        Assert.Equal("TEST-SKU", createdProduct.Sku);

        // Act - GET за отриманим Location
        var getResponse = await _client.GetAsync(postResponse.Headers.Location);

        // Assert - перевірка статусу 200 OK
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetchedProduct = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(fetchedProduct);
        Assert.Equal(createdProduct.Id, fetchedProduct.Id);
    }
}
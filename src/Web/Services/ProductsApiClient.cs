using System.Net;
using System.Text.Json;
using Contracts;

namespace Web.Services;

public sealed class ProductsApiClient(HttpClient http)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<global::Contracts.ProductDto>> ListAsync(
        string? search = null,
        string? sortBy = null,
        string? sortDir = null,
        CancellationToken ct = default)
    {
        var queryParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
            queryParts.Add($"search={Uri.EscapeDataString(search.Trim())}");
        if (!string.IsNullOrWhiteSpace(sortBy))
            queryParts.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
        if (!string.IsNullOrWhiteSpace(sortDir))
            queryParts.Add($"sortDir={Uri.EscapeDataString(sortDir)}");

        var url = "api/products" + (queryParts.Count > 0 ? "?" + string.Join("&", queryParts) : string.Empty);

        using var response = await http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(raw))
            return [];

        using var doc = JsonDocument.Parse(raw);
        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            return doc.RootElement.Deserialize<List<global::Contracts.ProductDto>>(SerializerOptions) ?? [];
        }

        var paged = doc.RootElement.Deserialize<PagedProductsResult>(SerializerOptions);

        return paged?.Items ?? [];
    }

    public async Task<global::Contracts.ProductDto?> FindAsync(string id, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"api/products/{Uri.EscapeDataString(id)}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<global::Contracts.ProductDto>(ct);
    }

    public Task<global::Contracts.ApiResult> CreateAsync(global::Contracts.ProductDto model, CancellationToken ct = default) =>
        SendAsync(HttpMethod.Post, "api/products", model, ct);

    public Task<global::Contracts.ApiResult> UpdateAsync(global::Contracts.ProductDto model, CancellationToken ct = default) =>
        SendAsync(HttpMethod.Put, $"api/products/{Uri.EscapeDataString(model.Id)}", model, ct);

    public async Task<global::Contracts.ApiResult> DeleteAsync(string id, CancellationToken ct = default)
    {
        try
        {
            using var response = await http.DeleteAsync($"api/products/{Uri.EscapeDataString(id)}", ct);
            if (response.IsSuccessStatusCode)
                return new global::Contracts.ApiResult(true);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return new global::Contracts.ApiResult(false, "Запис не знайдено — можливо, його вже видалили.");

            if (response.StatusCode == HttpStatusCode.Conflict)
                return new global::Contracts.ApiResult(false, "Конфлікт даних (409 Conflict): операція суперечить поточному стану.");

            return new global::Contracts.ApiResult(false, $"Сервер повернув {(int)response.StatusCode}.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return new global::Contracts.ApiResult(false, "API недоступний. Перевірте, чи запущено src/Api.");
        }
    }

    private async Task<global::Contracts.ApiResult> SendAsync(HttpMethod method, string url, global::Contracts.ProductDto model, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(new SaveProductRequest(model.Sku, model.Name, model.Unit, model.Quantity))
            };

            using var response = await http.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
                return new global::Contracts.ApiResult(true);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return new global::Contracts.ApiResult(false, ExtractProblemMessage(body) ?? "Запис не знайдено — можливо, його вже видалили.");

            if (response.StatusCode == HttpStatusCode.Conflict)
                return new global::Contracts.ApiResult(false, ExtractProblemMessage(body) ?? "Конфлікт даних (409 Conflict): операція суперечить поточному стану.");

            if (response.StatusCode != HttpStatusCode.BadRequest)
                return new global::Contracts.ApiResult(false, ExtractProblemMessage(body) ?? $"Сервер повернув {(int)response.StatusCode}.");

            try
            {
                var problem = JsonSerializer.Deserialize<ValidationProblemResponse>(body, SerializerOptions);

                if (problem?.Errors is { Count: > 0 })
                {
                    return new global::Contracts.ApiResult(
                        false,
                        problem.Title ?? "Дані не пройшли перевірку.",
                        problem.Errors
                    );
                }
            }
            catch (JsonException)
            {
            }

            var message = ExtractProblemMessage(body)
                ?? (string.IsNullOrWhiteSpace(body) ? "Дані не пройшли перевірку." : body.Trim('"'));

            return new global::Contracts.ApiResult(false, message);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return new global::Contracts.ApiResult(false, "API недоступний. Перевірте, чи запущено src/Api.");
        }
    }

    private static string? ExtractProblemMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return null;

            if (doc.RootElement.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(detail.GetString()))
                return detail.GetString();

            if (doc.RootElement.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(title.GetString()))
                return title.GetString();
        }
        catch (JsonException)
        {
        }

        return null;
    }
}

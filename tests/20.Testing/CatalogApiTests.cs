using System.Net;
using System.Net.Http.Json;
using Common.Domain;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Testing;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 20. Інтеграційні тести HTTP API
//
// WebApplicationFactory<TEntryPoint> піднімає РЕАЛЬНИЙ застосунок у пам'яті
// (весь конвеєр, DI, маршрутизація, серіалізація) і дає HttpClient, що
// звертається до нього без мережі. Це найдешевший спосіб перевірити поведінку
// endpoint-ів «як бачить клієнт».
//
// TEntryPoint тут — клас Program із прикладу 14 (тому там `public partial class Program;`).
// ─────────────────────────────────────────────────────────────────────────────

public sealed class CatalogApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Get_collection_returns_200_and_json_array()
    {
        var response = await _client.GetAsync("/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        Assert.NotNull(products);
        Assert.NotEmpty(products);
    }

    [Fact]
    public async Task Get_missing_item_returns_404()
    {
        var response = await _client.GetAsync("/products/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_creates_resource_and_returns_201_with_location()
    {
        var input = new ProductInput
        {
            Name = "Test Widget",
            Price = 9.99m,
            CategoryId = 3,
            Sku = "TST-WIDGET-1",
        };

        var response = await _client.PostAsJsonAsync("/products", input);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        // Ресурс справді доступний за Location.
        var follow = await _client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, follow.StatusCode);
        var created = await follow.Content.ReadFromJsonAsync<Product>();
        Assert.Equal("Test Widget", created!.Name);
    }

    [Fact]
    public async Task Post_with_invalid_body_returns_400_validation_problem()
    {
        var bad = new ProductInput { Name = "x", Price = 0, CategoryId = 1, Sku = "bad sku" };

        var response = await _client.PostAsJsonAsync("/products", bad);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("application/problem+json", response.Content.Headers.ContentType?.MediaType ?? "");
    }

    [Fact]
    public async Task Delete_is_idempotent_returns_204_every_time()
    {
        var input = new ProductInput { Name = "Doomed", Price = 1m, CategoryId = 3, Sku = "TST-DOOM-1" };
        var created = await _client.PostAsJsonAsync("/products", input);
        var location = created.Headers.Location!;

        var first = await _client.DeleteAsync(location);
        var second = await _client.DeleteAsync(location);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
    }

    [Fact]
    public async Task Put_is_idempotent_same_state_after_repeated_calls()
    {
        var body = new ProductInput
        {
            Name = "Repeatable", Description = "same", Price = 5m, CategoryId = 2, Sku = "TST-REPEAT-1",
        };

        var r1 = await _client.PutAsJsonAsync("/products/1", body);
        var s1 = await (await _client.GetAsync("/products/1")).Content.ReadFromJsonAsync<Product>();

        var r2 = await _client.PutAsJsonAsync("/products/1", body);
        var s2 = await (await _client.GetAsync("/products/1")).Content.ReadFromJsonAsync<Product>();

        Assert.Equal(HttpStatusCode.OK, r1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, r2.StatusCode);
        Assert.Equal(s1!.Name, s2!.Name);
        Assert.Equal(s1.Price, s2.Price);
    }

    [Fact]
    public async Task Wrong_method_on_item_returns_405()
    {
        var response = await _client.PostAsync("/products/1", content: null);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}

using System.Net;
using System.Net.Http.Json;
using Common.Domain;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Testing;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 20. Інтеграційні тести HTTP API
//
// WebApplicationFactory<TEntryPoint> піднімає РЕАЛЬНИЙ застосунок у пам'яті
// (весь конвеєр, DI, маршрутизація, серіалізація) і дає HttpClient без мережі.
// Найдешевший спосіб перевірити поведінку endpoint-ів «як бачить клієнт».
//
// TEntryPoint — клас Program із прикладу 14 (там є `public partial class Program;`).
// ─────────────────────────────────────────────────────────────────────────────

public sealed class CatalogApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Get_collection_returns_200()
    {
        var response = await _client.GetAsync("/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        Assert.NotEmpty(products!);
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
        var input = new ProductInput { Name = "Тестовий товар", Price = 9.99m, CategoryId = 3 };

        var response = await _client.PostAsJsonAsync("/products", input);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        // Ресурс справді доступний за Location.
        var created = await _client.GetFromJsonAsync<Product>(response.Headers.Location);
        Assert.Equal("Тестовий товар", created!.Name);
    }

    [Fact]
    public async Task Post_with_invalid_body_returns_400()
    {
        var bad = new ProductInput { Name = "x", Price = 0, CategoryId = 1 };

        var response = await _client.PostAsJsonAsync("/products", bad);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_is_idempotent_returns_204_every_time()
    {
        var created = await _client.PostAsJsonAsync("/products",
            new ProductInput { Name = "На видалення", Price = 1m, CategoryId = 3 });
        var location = created.Headers.Location!;

        var first = await _client.DeleteAsync(location);
        var second = await _client.DeleteAsync(location);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
    }
}

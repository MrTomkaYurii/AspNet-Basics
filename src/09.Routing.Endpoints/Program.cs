using Common;
using Common.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 09. Endpoint routing
//
// Маршрутизація в ASP.NET Core — двоетапна і сама складається з middleware:
//
//   UseRouting()   — дивиться на запит, СПІВСТАВЛЯЄ його з таблицею маршрутів
//                    і кладе обраний Endpoint у HttpContext (ще не виконує!).
//   << тут можна вставити middleware, що вже знає, який endpoint обрано >>
//   UseEndpoints() — ВИКОНУЄ обраний endpoint. У Minimal API викликається неявно.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCatalog();
var app = builder.Build();

// Явно вмикаємо маршрутизацію, щоб вставити middleware МІЖ співставленням і виконанням.
app.UseRouting();

app.Use(async (context, next) =>
{
    // GetEndpoint() != null означає, що UseRouting уже щось обрав.
    var endpoint = context.GetEndpoint();
    if (endpoint is not null)
    {
        context.Response.Headers["X-Matched-Endpoint"] = endpoint.DisplayName ?? "(без назви)";
        // Значення з шаблону маршруту вже розібрані:
        var routeValues = context.Request.RouteValues;
        if (routeValues.Count > 0)
            app.Logger.LogInformation("Route values: {Values}",
                string.Join(", ", routeValues.Select(kv => $"{kv.Key}={kv.Value}")));
    }
    await next(context);
});

app.MapGet("/", () => Results.Text(
    "Приклад 09. /products/1, /products/1/reviews/{guid}, /categories/laptops, /files/a/b/c, /search?q=hub"));

// ── Обмеження маршрутів (route constraints) ────────────────────────────────
// {id:int} — співпаде тільки якщо сегмент парситься як int. Інакше — 404,
// а не 400: маршрут просто "не той".
app.MapGet("/products/{id:int}", (int id, ICatalog catalog) =>
    catalog.FindProduct(id) is { } p ? Results.Ok(p) : Results.NotFound());

// Кілька параметрів + constraint :guid.
app.MapGet("/products/{id:int}/reviews/{reviewId:guid}", (int id, Guid reviewId) =>
    Results.Ok(new { productId = id, reviewId, text = "демо-відгук" }));

// :alpha — лише літери. Схоже на бізнес-ключ (slug).
app.MapGet("/categories/{slug:alpha:minlength(3)}", (string slug, ICatalog catalog) =>
    catalog.GetCategories().FirstOrDefault(c => c.Slug == slug) is { } cat
        ? Results.Ok(cat) : Results.NotFound());

// Необов'язковий параметр зі значенням за замовчуванням.
app.MapGet("/pages/{page:int=1}", (int page) => Results.Ok(new { page }));

// Catch-all: {**rest} захоплює решту шляху разом зі слешами.
app.MapGet("/files/{**path}", (string path) => Results.Ok(new { requestedPath = path }));

// Пріоритет: точний сегмент виграє в параметра навіть без явного Order.
app.MapGet("/products/featured", () => Results.Ok(new { note = "цей маршрут конкретніший за /products/{id}" }));

// ── Route values vs query string ──────────────────────────────────────────
// Шлях ідентифікує РЕСУРС; query — параметри операції над колекцією (фільтр, сортування).
app.MapGet("/search", (string? q, int page, int pageSize, ICatalog catalog) =>
{
    page = page <= 0 ? 1 : page;
    pageSize = pageSize is <= 0 or > 100 ? 20 : pageSize;
    var hits = catalog.GetProducts()
        .Where(p => string.IsNullOrWhiteSpace(q) || p.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
        .Skip((page - 1) * pageSize).Take(pageSize);
    return Results.Ok(new { q, page, pageSize, items = hits });
});

// ── Генерація посилань (LinkGenerator) ────────────────────────────────────
// Ніколи не склеюйте URL рядками — просіть у маршрутизатора за ІМЕНЕМ маршруту.
app.MapGet("/products/{id:int}/link", (int id, LinkGenerator links, HttpContext http) =>
{
    var self = links.GetUriByName(http, "product-by-id", new { id });
    return Results.Ok(new { self });
})
.WithName("product-link-demo");

app.MapGet("/named/{id:int}", (int id) => Results.Ok(new { id }))
   .WithName("product-by-id");

app.Run();

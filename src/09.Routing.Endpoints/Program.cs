using Common;
using Common.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 09. Endpoint routing
//
// Маршрутизація двоетапна і сама складається з middleware:
//   UseRouting()   — СПІВСТАВЛЯЄ запит із таблицею маршрутів, кладе Endpoint у HttpContext;
//   << тут middleware вже знає, який endpoint обрано >>
//   UseEndpoints() — ВИКОНУЄ обраний endpoint (у Minimal API викликається неявно).
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddCatalog();
builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();

// Явний UseRouting — щоб вставити middleware МІЖ співставленням і виконанням.
app.UseRouting();

app.Use(async (context, next) =>
{
    // Після UseRouting endpoint уже обрано (але ще не виконано).
    var endpoint = context.GetEndpoint();
    context.Response.Headers["X-Matched-Endpoint"] = endpoint?.DisplayName ?? "none";
    await next(context);
});

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", () => "Приклад 09. /products/1, /products/featured, /categories/laptops, /files/a/b/c");

// Constraint :int — несумісний сегмент дає 404 (маршрут «не той»), а не 400.
// .WithName(...) — ім'я маршруту для генерації URL (див. /link нижче).
app.MapGet("/products/{id:int}", (int id, ICatalog catalog) =>
    catalog.FindProduct(id) is { } p ? Results.Ok(p) : Results.NotFound())
    .WithName("product");

// Літеральний сегмент конкретніший за параметр → виграє в /products/{id}.
app.MapGet("/products/featured", () => "Спеціальний маршрут (конкретніший за /products/{id}).");

// Кілька параметрів + constraint :guid.
app.MapGet("/products/{id:int}/reviews/{reviewId:guid}", (int id, Guid reviewId) =>
    new { productId = id, reviewId });

// :alpha — лише літери (бізнес-ключ / slug).
app.MapGet("/categories/{slug:alpha}", (string slug, ICatalog catalog) =>
    catalog.GetCategories().FirstOrDefault(c => c.Slug == slug) is { } c ? Results.Ok(c) : Results.NotFound());

// Значення за замовчуванням.
app.MapGet("/pages/{page:int=1}", (int page) => new { page });

// Catch-all: {**path} захоплює решту шляху разом зі слешами.
app.MapGet("/files/{**path}", (string path) => new { path });

// Генерація URL: не склеюйте рядки — просіть у маршрутизатора за ІМЕНЕМ маршруту.
app.MapGet("/link/{id:int}", (int id, LinkGenerator links, HttpContext http) =>
    new { url = links.GetUriByName(http, "product", new { id }) });

app.Run();

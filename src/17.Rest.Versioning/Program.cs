using Asp.Versioning;
using Asp.Versioning.Conventions;
using Common;
using Common.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 17. REST: версіонування API
//
// Публічний API змінюється — але старі клієнти мають працювати. Рішення:
// кілька версій контракту одночасно. Пакет Asp.Versioning підтримує чотири
// способи вказати версію: сегмент URL, query, заголовок, media-type.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCatalog();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;      // без версії → v1
    options.ReportApiVersions = true;                        // заголовки api-supported-versions / api-deprecated-versions

    // Кілька читачів одночасно: клієнт обирає зручний спосіб.
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),                    // /v2/products
        new QueryStringApiVersionReader("api-version"),      // ?api-version=2.0
        new HeaderApiVersionReader("X-Api-Version"),         // X-Api-Version: 2.0
        new MediaTypeApiVersionReader("v"));                 // Accept: application/json;v=2.0
});

var app = builder.Build();

app.MapGet("/", () => Results.Text(
    "Приклад 17. /v1/products та /v2/products. Або /products із ?api-version=2.0 / заголовком."));

var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .HasApiVersion(new ApiVersion(2, 0))
    .Build();

// ── v1: «плоский» товар (як у Common.Product) ─────────────────────────────
app.MapGet("/v{version:apiVersion}/products", (ICatalog catalog) => catalog.GetProducts())
   .WithApiVersionSet(versionSet)
   .MapToApiVersion(new ApiVersion(1, 0));

// ── v2: змінений контракт — категорія розкрита, ціна як об'єкт ─────────────
app.MapGet("/v{version:apiVersion}/products", (ICatalog catalog) =>
        catalog.GetProducts().Select(p => new
        {
            p.Id,
            p.Name,
            category = catalog.FindCategory(p.CategoryId),          // v2: вкладений об'єкт замість CategoryId
            price = new { amount = p.Price, currency = "USD" },     // v2: гроші як структура
            p.Sku,
        }))
   .WithApiVersionSet(versionSet)
   .MapToApiVersion(new ApiVersion(2, 0));

// ── Приклад застарілої версії ────────────────────────────────────────────
app.MapGet("/v{version:apiVersion}/legacy-report", () => Results.Ok(new { note = "стара форма звіту" }))
   .WithApiVersionSet(app.NewApiVersionSet("legacy")
       .HasDeprecatedApiVersion(new ApiVersion(1, 0))   // → заголовок api-deprecated-versions
       .HasApiVersion(new ApiVersion(2, 0))
       .Build())
   .MapToApiVersion(new ApiVersion(1, 0));

// Той самий ресурс без сегмента версії — версія з query/заголовка/media-type.
app.MapGet("/products", (ICatalog catalog, HttpContext http) =>
{
    var version = http.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion?.ToString() ?? "1.0";
    return Results.Ok(new { version, items = catalog.GetProducts().Select(p => p.Name) });
})
.WithApiVersionSet(versionSet)
.HasApiVersion(new ApiVersion(1, 0))
.HasApiVersion(new ApiVersion(2, 0));

app.Run();

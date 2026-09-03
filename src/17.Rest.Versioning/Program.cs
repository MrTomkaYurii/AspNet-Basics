using Asp.Versioning;
using Asp.Versioning.Conventions;
using Common;
using Common.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 17. REST: версіонування API
//
// Публічний API змінюється, а старі клієнти мають працювати. Рішення: кілька
// версій контракту одночасно. Пакет Asp.Versioning підтримує 4 способи вказати
// версію: сегмент URL, query, заголовок, media-type.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddCatalog();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;   // заголовки api-supported-versions / api-deprecated-versions

    // Кілька читачів одночасно — клієнт обирає зручний спосіб.
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),                 // /v2/products
        new QueryStringApiVersionReader("api-version"),   // ?api-version=2.0
        new HeaderApiVersionReader("X-Api-Version"),      // X-Api-Version: 2.0
        new MediaTypeApiVersionReader("v"));             // Accept: application/json;v=2.0
});

builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", () => "Приклад 17. /v1/products та /v2/products; або /products із ?api-version=2.0.");

var versions = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .HasApiVersion(new ApiVersion(2))
    .Build();

// v1 — товар «як є».
app.MapGet("/v{version:apiVersion}/products", (ICatalog catalog) => catalog.GetProducts())
   .WithApiVersionSet(versions)
   .MapToApiVersion(new ApiVersion(1));

// v2 — змінений контракт: замість categoryId віддаємо назву категорії.
app.MapGet("/v{version:apiVersion}/products", (ICatalog catalog) =>
        catalog.GetProducts().Select(p => new
        {
            p.Id,
            p.Name,
            p.Price,
            Category = catalog.FindCategory(p.CategoryId)?.Name,
        }))
   .WithApiVersionSet(versions)
   .MapToApiVersion(new ApiVersion(2));

// Той самий ресурс без сегмента версії — версію беремо з query / заголовка / media-type.
app.MapGet("/products", (HttpContext http) =>
        new { version = http.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion?.ToString() ?? "1.0" })
   .WithApiVersionSet(versions)
   .HasApiVersion(new ApiVersion(1))
   .HasApiVersion(new ApiVersion(2));

// Застаріла версія → заголовок api-deprecated-versions.
app.MapGet("/v{version:apiVersion}/report", () => "стара форма звіту")
   .WithApiVersionSet(app.NewApiVersionSet("report")
       .HasDeprecatedApiVersion(new ApiVersion(1))
       .Build())
   .MapToApiVersion(new ApiVersion(1));

app.Run();

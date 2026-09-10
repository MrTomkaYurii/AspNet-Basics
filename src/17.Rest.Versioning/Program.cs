using Asp.Versioning;
using Common;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 17. REST: версіонування API
//
// Публічний API змінюється, а старі клієнти мають працювати. Рішення: кілька
// версій контракту одночасно. Пакет Asp.Versioning підтримує 4 способи вказати
// версію: сегмент URL, query, заголовок, media-type.
//
// Версії оголошують атрибути [ApiVersion] / [MapToApiVersion] на контролерах.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddCatalog();
builder.Services.AddControllers();

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
        new MediaTypeApiVersionReader("v"));              // Accept: application/json;v=2.0
})
.AddMvc();   // інтеграція з контролерами

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
app.MapControllers();

app.Run();

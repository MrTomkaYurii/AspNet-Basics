// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 15. REST: представлення ресурсу
//
//   • Content negotiation — один ресурс, різні формати (JSON / XML) за Accept.
//   • Налаштування серіалізації System.Text.Json.
//   • Умовні запити: ETag + If-None-Match (кеш) та If-Match (оптимістичне блокування).
//   • Заголовки кешування (Cache-Control).
//   • PATCH: JSON Patch (RFC 6902) та JSON Merge Patch (RFC 7386).
//
// Використовуємо контролери — content negotiation це механізм форматерів MVC.
// ─────────────────────────────────────────────────────────────────────────────

using Common;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCatalog();

builder.Services
    .AddControllers(options =>
    {
        // Якщо клієнт просить формат, якого ми не вміємо, — 406 замість «мовчки JSON».
        options.ReturnHttpNotAcceptable = true;
    })
    // Додає XML-форматери (input + output) поряд із JSON.
    .AddXmlSerializerFormatters()
    .AddJsonOptions(o =>
    {
        // Налаштування System.Text.Json для всього застосунку.
        o.JsonSerializerOptions.WriteIndented = true;
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

app.MapGet("/", () => Results.Text(
    "Приклад 15. GET /products/1 (спробуйте Accept: application/xml), ETag, If-Match, PATCH."));

app.MapControllers();
app.Run();

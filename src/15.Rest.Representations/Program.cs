using Common;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 15. REST: представлення ресурсу
//
//   • Content negotiation — один ресурс, різні формати (JSON / XML) за Accept.
//   • Умовні запити: ETag + If-None-Match (кеш) та If-Match (оптимістичне блокування).
//   • PATCH: JSON Patch (RFC 6902).
//
// На контролерах — content negotiation це механізм форматерів MVC.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddCatalog();

builder.Services
    .AddControllers(o => o.ReturnHttpNotAcceptable = true)   // просять невідомий формат → 406
    .AddXmlSerializerFormatters();                           // XML поряд із JSON

builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
// Обробники — у ProductsController (тека Controllers/).
app.MapGet("/", () => "Приклад 15. GET /products/1 (спробуйте Accept: application/xml), ETag, If-Match, PATCH.");
app.MapControllers();

app.Run();

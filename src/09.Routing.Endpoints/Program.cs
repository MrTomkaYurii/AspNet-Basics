using Common;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 09. Endpoint routing
//
// Маршрутизація двоетапна і сама складається з middleware:
//   UseRouting()   — СПІВСТАВЛЯЄ запит із таблицею маршрутів, кладе Endpoint у HttpContext;
//   << тут middleware вже знає, який endpoint обрано >>
//   UseEndpoints() — ВИКОНУЄ обраний endpoint (для контролерів — MapControllers).
//
// Обробники — у RoutingController: атрибутна маршрутизація ([HttpGet("шаблон")])
// з тими самими шаблонами, constraints і catch-all.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddCatalog();
builder.Services.AddControllers();
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
app.MapControllers();

app.Run();

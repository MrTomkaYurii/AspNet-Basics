using Common;
using Common.Domain;
using Microsoft.AspNetCore.Mvc;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 11. Minimal API: прив'язка параметрів і валідація
//
//   Частина 1. Звідки Minimal API бере значення параметрів хендлера.
//   Частина 2. Валідація вводу: вбудована (.NET 10) + endpoint filter.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddCatalog();
builder.Services.AddValidation();   // .NET 10: авто-валідація параметрів за DataAnnotations
builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", () => "Приклад 11. Див. /bind/* та POST /validate.");

// ── ЧАСТИНА 1. ДЖЕРЕЛА ПРИВ'ЯЗКИ ─────────────────────────────────────────────
// Правила за замовчуванням:
//   • ім'я = сегмент маршруту          → route values
//   • простий тип, не в маршруті        → query string
//   • складний тип                      → тіло запиту (JSON)
//   • зареєстрований у DI               → з контейнера

app.MapGet("/bind/route/{id:int}", (int id) => new { from = "route", id });

// query, зокрема масив (?tags=a&tags=b). q — nullable → необов'язковий;
// page має значення за замовчуванням; non-nullable без default був би обов'язковим (400, якщо немає).
app.MapGet("/bind/query", (string? q, string[] tags, int page = 1) => new { from = "query", q, tags, page });

// заголовок — явно через атрибут
app.MapGet("/bind/header", ([FromHeader(Name = "X-Tenant")] string tenant) => new { from = "header", tenant });

// [AsParameters] — одна структура, зібрана з route + query
app.MapGet("/bind/search/{page:int}", ([AsParameters] SearchQuery query) => query);

// ── ЧАСТИНА 2. ВАЛІДАЦІЯ ─────────────────────────────────────────────────────
// AddValidation() робить так, що ProductInput перевіряється за своїми
// DataAnnotations ДО виклику хендлера (погана name/price → 400 ValidationProblem).
// Endpoint filter — для правил, яких немає в атрибутах (тут: категорія існує?).
app.MapPost("/validate", (ProductInput input) => $"OK: {input.Name}")
   .AddEndpointFilter(async (ctx, next) =>
   {
       var input = ctx.GetArgument<ProductInput>(0);
       var catalog = ctx.HttpContext.RequestServices.GetRequiredService<ICatalog>();

       if (catalog.FindCategory(input.CategoryId) is null)
           return Results.BadRequest("Категорії з таким Id немає.");

       return await next(ctx);
   });

app.Run();

// Модель для [AsParameters]: Page — з маршруту, решта — з query.
record SearchQuery(int Page, string? Term, int PageSize = 20);

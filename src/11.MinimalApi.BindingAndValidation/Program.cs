using Common;
using Common.Domain;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.BindingAndValidation;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 11. Minimal API: прив'язка параметрів і валідація
//
//   Частина 1. Звідки Minimal API бере значення параметрів хендлера.
//   Частина 2. Валідація вводу: вбудована (.NET 10) і власний endpoint filter.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCatalog();

// .NET 10: автоматична валідація параметрів Minimal API за DataAnnotations.
// Складні параметри з [Required]/[Range]/... перевіряються перед викликом
// хендлера; при помилці повертається 400 ValidationProblem.
builder.Services.AddValidation();

var app = builder.Build();

app.MapGet("/", () => Results.Text("Приклад 11. Див. /bind/* та /validate*."));

// ── ЧАСТИНА 1. ДЖЕРЕЛА ПРИВ'ЯЗКИ ───────────────────────────────────────────

// Правила за замовчуванням:
//   • назва збігається із сегментом маршруту  → з route values
//   • простий тип (int, string, Guid, DateOnly, типи з TryParse) і НЕ в маршруті → з query
//   • складний тип                            → з тіла запиту (JSON)
//   • тип, зареєстрований у DI                → із контейнера
//   • HttpContext / HttpRequest / ...         → напряму

// route → int id
app.MapGet("/bind/route/{id:int}", (int id) => new { source = "route", id });

// query → кілька значень, зокрема масив (?tags=a&tags=b) і DateOnly
app.MapGet("/bind/query", (string? q, int page, string[] tags, DateOnly? since) =>
    new { source = "query", q, page, tags, since });

// header → явно через атрибут
app.MapGet("/bind/header", ([FromHeader(Name = "X-Tenant")] string tenant) =>
    new { source = "header", tenant });

// [AsParameters] → одна структура, зібрана з route + query
app.MapGet("/bind/as-parameters/{page:int}", ([AsParameters] ListProductsQuery query, ICatalog catalog) =>
{
    var items = catalog.GetProducts()
        .Where(p => string.IsNullOrEmpty(query.Search) || p.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase))
        .Take(query.PageSize <= 0 ? 20 : query.PageSize);
    return new { query, items };
});

// власний тип із TryParse → з route
app.MapGet("/bind/geo/{point}", (GeoPoint point) => new { source = "route+TryParse", point });

// явні атрибути, коли треба перевизначити правила
app.MapPost("/bind/explicit", (
    [FromQuery] string mode,
    [FromBody] ProductInput input,
    [FromServices] ICatalog catalog) => new { mode, input.Name, categoryExists = catalog.FindCategory(input.CategoryId) is not null });

// ── ЧАСТИНА 2. ВАЛІДАЦІЯ ───────────────────────────────────────────────────

// Вбудована валідація: ProductInput має DataAnnotations → 400 без жодного коду тут.
app.MapPost("/validate/auto", (ProductInput input) => TypedResults.Ok(new { accepted = input.Name }));

// Той самий ефект «руками» через endpoint filter — корисно розуміти механізм
// і потрібно для складних крос-польових правил.
app.MapPost("/validate/filter", (ProductInput input, ICatalog catalog) =>
        TypedResults.Ok(new { accepted = input.Name }))
   .AddEndpointFilter(async (ctx, next) =>
   {
       var input = ctx.GetArgument<ProductInput>(0);
       var catalog = ctx.HttpContext.RequestServices.GetRequiredService<ICatalog>();

       var errors = new Dictionary<string, string[]>();
       var vc = new System.ComponentModel.DataAnnotations.ValidationContext(input);
       var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
       if (!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(input, vc, results, true))
           foreach (var r in results)
               errors[r.MemberNames.FirstOrDefault() ?? ""] = [r.ErrorMessage ?? "invalid"];

       // Крос-польове правило, якого немає в атрибутах:
       if (catalog.FindCategory(input.CategoryId) is null)
           errors[nameof(ProductInput.CategoryId)] = ["Категорії з таким Id немає."];

       return errors.Count > 0 ? Results.ValidationProblem(errors) : await next(ctx);
   });

app.Run();

using Common;
using Common.Domain;
using Rest.CollectionsAndErrors;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 16. REST: колекції та помилки
//
//   • Фільтрація + пагінація — параметри в query string, не в шляху.
//   • Конверт відповіді + метадані (X-Total-Count у заголовку).
//   • Єдиний формат помилок: ProblemDetails (RFC 9457).
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddCatalog();
builder.Services.AddProblemDetails();
builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();
app.UseStatusCodePages();

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", () => "Приклад 16. GET /products?q=&page=1&pageSize=5");

// Колекція: query задає фільтр і сторінку. Шлях (/products) — це ідентичність колекції.
app.MapGet("/products", ([AsParameters] ProductQuery query, ICatalog catalog, HttpResponse response) =>
{
    if (query.PageSize is < 1 or > 100)
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["pageSize"] = ["Має бути в діапазоні 1..100."],
        });

    var filtered = catalog.GetProducts()
        .Where(p => string.IsNullOrWhiteSpace(query.Q)
                 || p.Name.Contains(query.Q, StringComparison.OrdinalIgnoreCase))
        .ToList();

    var pageNumber = Math.Max(query.Page, 1);
    var items = filtered
        .Skip((pageNumber - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToList();

    response.Headers["X-Total-Count"] = filtered.Count.ToString();
    return Results.Ok(new Page<Product>(items, pageNumber, query.PageSize, filtered.Count));
});

// Помилка «немає ресурсу» — у форматі ProblemDetails, з додатковим полем.
app.MapGet("/products/{id:int}", (int id, ICatalog catalog) =>
    catalog.FindProduct(id) is { } p
        ? Results.Ok(p)
        : Results.Problem(
            title: "Товар не знайдено",
            statusCode: StatusCodes.Status404NotFound,
            extensions: new Dictionary<string, object?> { ["productId"] = id }));

app.Run();

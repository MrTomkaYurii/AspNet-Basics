using Common;
using Common.Domain;
using Rest.CollectionsAndErrors;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 16. REST: колекції та помилки
//
//   • Пагінація, сортування, фільтрація — параметри в query.
//   • Конверт відповіді + метадані в заголовках (X-Total-Count, Link).
//   • Єдиний формат помилок: ProblemDetails (RFC 9457) та ValidationProblem.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCatalog();

// Вмикає ProblemDetails і для винятків, і для «голих» статус-кодів фреймворку.
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapGet("/", () => Results.Text(
    "Приклад 16. GET /products?q=&category=&minPrice=&maxPrice=&sort=-price&page=1&pageSize=5"));

app.MapGet("/products", (
    [AsParameters] ProductQuery query,
    ICatalog catalog,
    HttpContext http) =>
{
    // ── Валідація параметрів колекції → 400 ValidationProblem ──────────────
    var errors = new Dictionary<string, string[]>();

    if (query.PageSize is > ProductQuery.MaxPageSize)
        errors["pageSize"] = [$"Максимум {ProductQuery.MaxPageSize}."];
    if (query.Page is < 1)
        errors["page"] = ["Має бути ≥ 1."];
    if (query.MinPrice is < 0)
        errors["minPrice"] = ["Не може бути від'ємною."];

    var sortField = (query.Sort ?? "id").TrimStart('-');
    if (!ProductQuery.AllowedSortFields.Contains(sortField))
        errors["sort"] = [$"Дозволені поля: {string.Join(", ", ProductQuery.AllowedSortFields)}."];

    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    // ── Фільтрація ───────────────────────────────────────────────────────
    IEnumerable<Product> items = catalog.GetProducts();

    if (!string.IsNullOrWhiteSpace(query.Q))
        items = items.Where(p => p.Name.Contains(query.Q, StringComparison.OrdinalIgnoreCase));
    if (!string.IsNullOrWhiteSpace(query.Category))
        items = items.Where(p => catalog.FindCategory(p.CategoryId)?.Slug == query.Category);
    if (query.MinPrice is { } min)
        items = items.Where(p => p.Price >= min);
    if (query.MaxPrice is { } max)
        items = items.Where(p => p.Price <= max);

    // ── Сортування (префікс '-' = за спаданням) ───────────────────────────
    var desc = query.Sort?.StartsWith('-') == true;
    Func<Product, object> key = sortField switch
    {
        "name" => p => p.Name,
        "price" => p => p.Price,
        "version" => p => p.Version,
        _ => p => p.Id,
    };
    items = desc ? items.OrderByDescending(key) : items.OrderBy(key);

    // ── Пагінація ────────────────────────────────────────────────────────
    var all = items.ToList();
    var pageItems = all
        .Skip((query.SafePage - 1) * query.SafePageSize)
        .Take(query.SafePageSize)
        .ToList();

    var result = new PagedResult<Product>(pageItems, query.SafePage, query.SafePageSize, all.Count);

    // Метадублі в заголовках — зручно для клієнтів, що не читають тіло.
    http.Response.Headers["X-Total-Count"] = result.Total.ToString();
    http.Response.Headers["Link"] = BuildLinkHeader(http.Request, result);

    return Results.Ok(result);
});

// Помилка «немає ресурсу» — теж у форматі ProblemDetails, з деталями.
app.MapGet("/products/{id:int}", (int id, ICatalog catalog) =>
    catalog.FindProduct(id) is { } p
        ? Results.Ok(p)
        : Results.Problem(
            title: "Товар не знайдено",
            detail: $"Товару з id={id} не існує.",
            statusCode: StatusCodes.Status404NotFound,
            extensions: new Dictionary<string, object?> { ["productId"] = id }));

app.Run();

// Заголовок Link (RFC 8288) з посиланнями на сусідні сторінки.
static string BuildLinkHeader(HttpRequest req, PagedResult<Product> page)
{
    string Url(int p)
    {
        var q = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(req.QueryString.Value ?? "")
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
        q["page"] = p.ToString();
        q["pageSize"] = page.PageSize.ToString();
        var qs = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(
            $"{req.Scheme}://{req.Host}{req.Path}", q!);
        return qs;
    }

    var links = new List<string> { $"<{Url(1)}>; rel=\"first\"", $"<{Url(page.TotalPages)}>; rel=\"last\"" };
    if (page.HasPrev) links.Add($"<{Url(page.Page - 1)}>; rel=\"prev\"");
    if (page.HasNext) links.Add($"<{Url(page.Page + 1)}>; rel=\"next\"");
    return string.Join(", ", links);
}

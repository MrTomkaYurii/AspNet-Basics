using Common;
using Common.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 10. Minimal API: повний CRUD
//
// Minimal API — компактний спосіб оголошувати endpoint-и без класів-контролерів.
// Тут: MapGroup (спільний префікс), правильні коди відповідей, заголовок Location,
// впровадження залежностей прямо в параметри хендлера.
// (Валідацію вводу навмисно не робимо — це тема прикладу 11.)
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddCatalog();
builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", () => Results.Redirect("/products"));

// MapGroup: усі endpoint-и нижче мають префікс /products.
var products = app.MapGroup("/products").WithTags("Products");

// LIST → 200 + масив
products.MapGet("/", (ICatalog catalog) => catalog.GetProducts());

// READ → 200 або 404
products.MapGet("/{id:int}", (int id, ICatalog catalog) =>
    catalog.FindProduct(id) is { } p ? Results.Ok(p) : Results.NotFound());

// CREATE → 201 + заголовок Location на новий ресурс
products.MapPost("/", (ProductInput input, ICatalog catalog) =>
{
    if (catalog.FindCategory(input.CategoryId) is null)
        return Results.BadRequest("Категорії з таким Id немає.");

    var created = catalog.Add(input);
    return Results.Created($"/products/{created.Id}", created);
});

// REPLACE (повна заміна) → 200 або 404. Ідемпотентний.
products.MapPut("/{id:int}", (int id, ProductInput input, ICatalog catalog) =>
    catalog.Replace(id, input) is { } updated ? Results.Ok(updated) : Results.NotFound());

// DELETE → 204. Ідемпотентний: 204 навіть якщо вже видалено.
products.MapDelete("/{id:int}", (int id, ICatalog catalog) =>
{
    catalog.Delete(id);
    return Results.NoContent();
});

// Довідник категорій.
app.MapGet("/categories", (ICatalog catalog) => catalog.GetCategories()).WithTags("Categories");

app.Run();

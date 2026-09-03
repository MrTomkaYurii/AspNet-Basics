using Common;
using Common.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 10. Minimal API: повний CRUD
//
// Minimal API — компактний спосіб оголошувати endpoint-и без контролерів.
// Тут показано:
//   • групи маршрутів (MapGroup) зі спільним префіксом і метаданими;
//   • TypedResults та union-типи результатів (Results<Ok<T>, NotFound>);
//   • впровадження залежностей прямо в параметри хендлера;
//   • правильні коди відповідей і заголовок Location для POST.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCatalog();
var app = builder.Build();

app.MapGet("/", () => Results.Redirect("/products"));

// MapGroup: усі endpoint-и нижче матимуть префікс /products і спільний тег.
var products = app.MapGroup("/products").WithTags("Products");

// LIST — GET /products
products.MapGet("/", (ICatalog catalog) => TypedResults.Ok(catalog.GetProducts()));

// READ — GET /products/{id}
// Union-тип: компілятор і OpenAPI знають про обидва можливі результати.
products.MapGet("/{id:int}", Results<Ok<Product>, NotFound> (int id, ICatalog catalog)
    => catalog.FindProduct(id) is { } p ? TypedResults.Ok(p) : TypedResults.NotFound())
    .WithName("GetProductById");

// CREATE — POST /products
// 201 Created + заголовок Location на новий ресурс + тіло створеного об'єкта.
products.MapPost("/", Results<Created<Product>, ValidationProblem> (
    ProductInput input, ICatalog catalog) =>
{
    if (!MiniValidator.TryValidate(input, out var errors))
        return TypedResults.ValidationProblem(errors);

    if (catalog.FindCategory(input.CategoryId) is null)
        return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(ProductInput.CategoryId)] = ["Категорії з таким Id немає."],
        });

    var created = catalog.Add(input);
    return TypedResults.Created($"/products/{created.Id}", created);
});

// REPLACE — PUT /products/{id}  (повна заміна, ідемпотентна)
products.MapPut("/{id:int}", Results<Ok<Product>, NotFound, ValidationProblem> (
    int id, ProductInput input, ICatalog catalog) =>
{
    if (!MiniValidator.TryValidate(input, out var errors))
        return TypedResults.ValidationProblem(errors);

    return catalog.Replace(id, input) is { } updated
        ? TypedResults.Ok(updated)
        : TypedResults.NotFound();
});

// DELETE — DELETE /products/{id}  (ідемпотентна: 204 навіть якщо вже видалено)
products.MapDelete("/{id:int}", NoContent (int id, ICatalog catalog) =>
{
    catalog.Delete(id);
    return TypedResults.NoContent();
});

// Довідник категорій.
app.MapGet("/categories", (ICatalog catalog) => TypedResults.Ok(catalog.GetCategories()))
   .WithTags("Categories");

app.Run();

// ─────────────────────────────────────────────────────────────────────────────
// Крихітний валідатор DataAnnotations без зовнішніх пакетів — щоб приклад
// лишався самодостатнім. У реальному проекті беруть MiniValidation або
// вбудовану валідацію Minimal API (див. приклад 11).
// ─────────────────────────────────────────────────────────────────────────────
file static class MiniValidator
{
    public static bool TryValidate(object model, out Dictionary<string, string[]> errors)
    {
        var ctx = new System.ComponentModel.DataAnnotations.ValidationContext(model);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var ok = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, ctx, results, true);

        errors = results
            .SelectMany(r => r.MemberNames.DefaultIfEmpty(""), (r, m) => (member: m, error: r.ErrorMessage ?? "invalid"))
            .GroupBy(x => x.member)
            .ToDictionary(g => g.Key, g => g.Select(x => x.error).ToArray());

        return ok;
    }
}

using Common;
using Common.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 14. REST: ресурси, дієслова, коди відповідей, ідемпотентність
//
// REST — це архітектурний стиль. Ключові ідеї:
//   • РЕСУРС має стабільний URI-ідентифікатор (/products/42);
//   • над ресурсом виконують СТАНДАРТНІ операції HTTP-дієсловами;
//   • сервер відповідає осмисленим СТАТУС-КОДОМ і представленням;
//   • взаємодія БЕЗ СТАНУ (кожен запит самодостатній).
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCatalog();
builder.Services.AddProblemDetails();
var app = builder.Build();

app.UseStatusCodePages();   // тіло для 404/405/415 тощо

app.MapGet("/", () => Results.Ok(new
{
    resource = "/products",
    verbs = new object[]
    {
        new { method = "GET",    on = "collection/item", idempotent = true,  safe = true,  effect = "читання" },
        new { method = "POST",   on = "collection",      idempotent = false, safe = false, effect = "створити новий елемент → 201 + Location" },
        new { method = "PUT",    on = "item",            idempotent = true,  safe = false, effect = "повна заміна елемента" },
        new { method = "PATCH",  on = "item",            idempotent = false, safe = false, effect = "часткова зміна (див. приклад 15)" },
        new { method = "DELETE", on = "item",            idempotent = true,  safe = false, effect = "видалити елемент → 204" },
    },
}));

var products = app.MapGroup("/products");

// ── GET колекція → завжди 200 (порожній масив — це теж успіх) ───────────────
products.MapGet("/", (ICatalog catalog) => TypedResults.Ok(catalog.GetProducts()));

// ── GET елемент → 200 або 404 ─────────────────────────────────────────────
products.MapGet("/{id:int}", Results<Ok<Product>, NotFound> (int id, ICatalog catalog)
    => catalog.FindProduct(id) is { } p ? TypedResults.Ok(p) : TypedResults.NotFound());

// ── POST на колекцію → 201 Created + Location; НЕ ідемпотентний ─────────────
products.MapPost("/", Results<Created<Product>, ValidationProblem> (ProductInput input, ICatalog catalog) =>
{
    if (Validate(input, catalog) is { } errors)
        return TypedResults.ValidationProblem(errors);

    var created = catalog.Add(input);
    // 201 + абсолютний/відносний URI нового ресурсу в Location.
    return TypedResults.Created($"/products/{created.Id}", created);
});

// ── PUT на елемент → повна заміна; ІДЕМПОТЕНТНИЙ ───────────────────────────
// Той самий запит, надісланий двічі, лишає систему в тому самому стані.
products.MapPut("/{id:int}", Results<Ok<Product>, NotFound, ValidationProblem> (
    int id, ProductInput input, ICatalog catalog) =>
{
    if (Validate(input, catalog) is { } errors)
        return TypedResults.ValidationProblem(errors);

    return catalog.Replace(id, input) is { } updated
        ? TypedResults.Ok(updated)
        : TypedResults.NotFound();
});

// ── DELETE елемент → 204 No Content; ІДЕМПОТЕНТНИЙ ────────────────────────
// Видалення вже видаленого — не помилка. Клієнт хотів «щоб цього не було» — так і є.
products.MapDelete("/{id:int}", NoContent (int id, ICatalog catalog) =>
{
    catalog.Delete(id);
    return TypedResults.NoContent();
});

// ── PATCH: заявляємо метод, але деталі — у прикладі 15 ────────────────────
products.MapPatch("/{id:int}", (int id) => Results.Problem(
    statusCode: StatusCodes.Status501NotImplemented,
    title: "PATCH розглянуто в прикладі 15 (JSON Patch / merge)."));

app.Run();

static Dictionary<string, string[]>? Validate(ProductInput input, ICatalog catalog)
{
    var errors = new Dictionary<string, string[]>();
    var vc = new System.ComponentModel.DataAnnotations.ValidationContext(input);
    var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
    if (!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(input, vc, results, true))
        foreach (var r in results)
            errors[r.MemberNames.FirstOrDefault() ?? ""] = [r.ErrorMessage ?? "invalid"];

    if (catalog.FindCategory(input.CategoryId) is null)
        errors[nameof(ProductInput.CategoryId)] = ["Категорії з таким Id немає."];

    return errors.Count > 0 ? errors : null;
}

// Потрібно для інтеграційних тестів (приклад 20): WebApplicationFactory<Program>.
public partial class Program;

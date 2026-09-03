using Common;
using Common.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 14. REST: ресурси, дієслова, коди відповідей, ідемпотентність
//
// REST — архітектурний стиль. Ключові ідеї:
//   • РЕСУРС має стабільний URI (/products/42);
//   • над ним — СТАНДАРТНІ операції HTTP-дієсловами;
//   • сервер відповідає осмисленим СТАТУС-КОДОМ;
//   • взаємодія БЕЗ СТАНУ (кожен запит самодостатній).
//
// Семантика дієслів — у NOTES.md.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddCatalog();
builder.Services.AddProblemDetails();
builder.Services.AddValidation();     // ProductInput перевіряється автоматично
builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();
app.UseStatusCodePages();             // тіло для 404 / 405 / 415

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
var products = app.MapGroup("/products");

// GET колекція → завжди 200 (порожній масив — теж успіх).
products.MapGet("/", (ICatalog catalog) => catalog.GetProducts());

// GET елемент → 200 або 404.
products.MapGet("/{id:int}", (int id, ICatalog catalog) =>
    catalog.FindProduct(id) is { } p ? Results.Ok(p) : Results.NotFound());

// POST на колекцію → 201 Created + Location. НЕ ідемпотентний.
products.MapPost("/", (ProductInput input, ICatalog catalog) =>
{
    if (catalog.FindCategory(input.CategoryId) is null)
        return Results.BadRequest("Категорії з таким Id немає.");

    var created = catalog.Add(input);
    return Results.Created($"/products/{created.Id}", created);
});

// PUT на елемент → повна заміна. ІДЕМПОТЕНТНИЙ: той самий запит двічі → той самий стан.
products.MapPut("/{id:int}", (int id, ProductInput input, ICatalog catalog) =>
    catalog.Replace(id, input) is { } updated ? Results.Ok(updated) : Results.NotFound());

// DELETE → 204. ІДЕМПОТЕНТНИЙ: видалення вже видаленого — теж 204, а не помилка.
products.MapDelete("/{id:int}", (int id, ICatalog catalog) =>
{
    catalog.Delete(id);
    return Results.NoContent();
});

// PATCH — часткова зміна. Деталі (JSON Patch / merge) — у прикладі 15.
products.MapPatch("/{id:int}", () => Results.StatusCode(StatusCodes.Status501NotImplemented));

app.Run();

// Потрібно для інтеграційних тестів (приклад 20): WebApplicationFactory<Program>.
public partial class Program;

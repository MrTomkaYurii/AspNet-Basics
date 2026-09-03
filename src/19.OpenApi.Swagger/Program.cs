using System.Text.Json.Nodes;
using Common;
using Common.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 19. OpenAPI та Swagger UI
//
// OpenAPI — машиночитний опис HTTP API (шляхи, параметри, схеми, коди).
// З нього генерують документацію, клієнтські SDK, моки, тести контракту.
//
// .NET 10 генерує документ вбудовано (Microsoft.AspNetCore.OpenApi).
// Swagger UI тут — лише візуалізація готового /openapi/v1.json.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCatalog();

builder.Services.AddOpenApi(options =>
{
    // Document transformer — правимо метадані всього документа.
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info = new()
        {
            Title = "Catalog API",
            Version = "v1",
            Description = "Навчальний приклад OpenAPI з курсу AspNet-Basics.",
            Contact = new() { Name = "AspNet-Basics", Url = new("https://learn.microsoft.com/aspnet/core") },
        };
        return Task.CompletedTask;
    });

    // Schema transformer — додаємо приклад до схеми ProductInput.
    options.AddSchemaTransformer((schema, context, ct) =>
    {
        if (context.JsonTypeInfo.Type == typeof(ProductInput))
        {
            schema.Example = new JsonObject
            {
                ["name"] = "Wireless Mouse M2",
                ["description"] = "Тиха бездротова миша",
                ["price"] = 24.99,
                ["categoryId"] = 2,
                ["sku"] = "ACC-MOUSE-M2",
            };
        }
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// /openapi/v1.json — сам документ.
app.MapOpenApi();

// Swagger UI на /swagger, що читає згенерований документ.
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Catalog API v1");
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"));

var products = app.MapGroup("/products")
    .WithTags("Products");

products.MapGet("/", (ICatalog catalog) => catalog.GetProducts())
    .WithSummary("Список товарів")
    .WithDescription("Повертає всі товари каталогу. Пагінацію див. у прикладі 16.")
    .Produces<IEnumerable<Product>>();

products.MapGet("/{id:int}", Results<Ok<Product>, NotFound> (int id, ICatalog catalog) =>
        catalog.FindProduct(id) is { } p ? TypedResults.Ok(p) : TypedResults.NotFound())
    .WithSummary("Отримати товар за Id")
    .WithName("GetProduct");

products.MapPost("/", Results<Created<Product>, ValidationProblem> (ProductInput input, ICatalog catalog) =>
    {
        if (catalog.FindCategory(input.CategoryId) is null)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(ProductInput.CategoryId)] = ["Категорії з таким Id немає."],
            });
        var created = catalog.Add(input);
        return TypedResults.Created($"/products/{created.Id}", created);
    })
    .WithSummary("Створити товар")
    .WithDescription("Повертає 201 Created із заголовком Location.");

app.MapGet("/categories", (ICatalog catalog) => catalog.GetCategories())
    .WithTags("Categories")
    .WithSummary("Довідник категорій");

app.Run();

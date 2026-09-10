using System.Text.Json.Nodes;
using Common;
using Common.Domain;
using Scalar.AspNetCore;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 19. OpenAPI + Swagger UI + Scalar
//
// OpenAPI — машиночитний опис HTTP API. З нього генерують документацію,
// клієнтські SDK, тести контракту.
//
// .NET 10 генерує документ вбудовано (AddOpenApi / MapOpenApi). Swagger UI та
// Scalar — лише переглядачі поверх того самого /openapi/v1.json; працюють паралельно.
//
// Обробники — у ProductsController / CategoriesController. Опис (summary, коди
// відповідей) береться з /// XML-коментарів і атрибутів [ProducesResponseType].
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddCatalog();
builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    // Document transformer — метадані всього документа.
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Catalog API";
        document.Info.Description = "Навчальний приклад OpenAPI з курсу AspNet-Basics.";
        return Task.CompletedTask;
    });

    // Schema transformer — приклад тіла для ProductInput у UI.
    options.AddSchemaTransformer((schema, context, _) =>
    {
        if (context.JsonTypeInfo.Type == typeof(ProductInput))
            schema.Example = new JsonObject { ["name"] = "Бездротова миша", ["price"] = 24.99, ["categoryId"] = 2 };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР + ДОКУМЕНТАЦІЯ
// ======================================================================
app.MapOpenApi();                                          // /openapi/v1.json — джерело правди
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Catalog API"));   // /swagger
app.MapScalarApiReference(o => o.WithOpenApiRoutePattern("/openapi/v1.json")); // /scalar
app.OpenBrowserOnStart();

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", () => Results.Content(
    "<h1>Приклад 19</h1><ul><li><a href='/swagger'>/swagger</a></li><li><a href='/scalar'>/scalar</a></li></ul>",
    "text/html"));

app.MapControllers();

app.Run();

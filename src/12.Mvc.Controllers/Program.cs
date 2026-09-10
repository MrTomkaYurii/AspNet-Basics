using Common;
using Scalar.AspNetCore;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 12. MVC-контролери для Web API: розбір [ApiController]
//
// Приклади 07–11 уже на контролерах. Тут детально: що вмикає [ApiController]
// (авто-400, вивід джерела прив'язки, ProblemDetails), ActionResult<T>,
// CreatedAtAction, ModelState для крос-польових правил.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddCatalog();
builder.Services.AddControllers();   // інфраструктура MVC: активація, binding, форматери

// Документація підключена НАПРЯМУ, без помічника Common.ApiDocs (розбір — приклад 01):
//   • AddOpenApi()  — генератор документа /openapi/v1.json;
//   • MapScalarApiReference() нижче — переглядач цього документа на /scalar.
// builder.Services.AddApiDocs();   // ← замінено прямим викликом
builder.Services.AddOpenApi();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
// app.MapApiDocs();   // ← замінено секцією нижче
app.MapOpenApi();                                                    // GET /openapi/v1.json
app.MapScalarApiReference(o => o.WithOpenApiRoutePattern("/openapi/v1.json"));  // GET /scalar

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
// Самі обробники — у класах-контролерах (тека Controllers/).
app.MapGet("/", () => Results.Redirect("/api/products"));
app.MapControllers();   // додає дії контролерів до таблиці маршрутів

app.Run();

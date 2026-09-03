using Common;
using Mvc.Filters.Filters;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 13. Фільтри MVC
//
// Фільтри — «конвеєр усередині конвеєра», специфічний для MVC. На відміну від
// middleware, вони знають про обрану дію, її аргументи та результат.
//
// Порядок типів (зовні → всередину):
//   Authorization → Resource → [model binding] → Action → [ДІЯ] → Result
//   Exception огортає Action + саму дію.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddControllers();

// Фільтри з залежностями реєструють у DI (для [ServiceFilter]).
builder.Services.AddScoped<DemoResourceFilter>();
builder.Services.AddScoped<DemoActionFilter>();
builder.Services.AddScoped<DemoResultFilter>();
builder.Services.AddScoped<DemoExceptionFilter>();

builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", () => "Приклад 13. GET /api/demo/ok, /api/demo/boom");
app.MapControllers();

app.Run();

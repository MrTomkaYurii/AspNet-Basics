using Mvc.Filters.Filters;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 13. Фільтри MVC
//
// Фільтри — це «конвеєр усередині конвеєра», специфічний для MVC. На відміну від
// middleware, вони знають про обрану дію, її аргументи та результат.
//
// Порядок типів фільтрів (зовні → всередину):
//
//   Authorization  →  Resource  →  [model binding]  →  Action  →  [дія]
//                                                          ↓
//   Result  ←  ...  ←  Exception (огортає Action + дію)
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    // ГЛОБАЛЬНИЙ фільтр — для всіх контролерів. Тут просто вимикаємо стандартний
    // ProblemDetails-фільтр не будемо; лишаємо як приклад місця реєстрації.
    // options.Filters.Add<SomeGlobalFilter>();
});

// Фільтри з залежностями реєструють у DI (для [ServiceFilter]).
builder.Services.AddScoped<TimingResourceFilter>();
builder.Services.AddScoped<LoggingActionFilter>();
builder.Services.AddScoped<EnvelopeResultFilter>();
builder.Services.AddScoped<DemoExceptionFilter>();

var app = builder.Build();

app.MapGet("/", () => Results.Text("Приклад 13. GET /api/demo/ok, /api/demo/boom, /api/demo/slow"));
app.MapControllers();

app.Run();

using Common;
using Pipeline.CustomMiddleware.Middleware;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 04. Власні middleware як класи — два стилі
//
//   1. RequestTimingMiddleware  — «за конвенцією» (звичайний клас).
//   2. CorrelationIdMiddleware  — на основі інтерфейсу IMiddleware.
//
// Коли middleware треба перевикористовувати чи тестувати — його виносять у клас.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
// IMiddleware-реалізацію реєструвати ОБОВ'ЯЗКОВО. Convention-based — ні.
builder.Services.AddScoped<CorrelationIdMiddleware>();
builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();

// Порядок реєстрації = порядок виконання. UseXxx() — метод-розширення, що
// ховає app.UseMiddleware<T>() за зрозумілою назвою (як UseRouting, UseCors).
app.UseCorrelationId();
app.UseRequestTiming();

// Третій варіант — інлайн, прямо тут. Годиться для дрібниць.
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Powered-By"] = "AspNet-Basics";
    await next(context);
});

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", (HttpContext http) => new
{
    message = "Дивіться заголовки відповіді: X-Correlation-ID, X-Powered-By.",
    correlationId = http.Items["CorrelationId"],
});

app.Run();

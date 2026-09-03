using Pipeline.CustomMiddleware.Middleware;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 04. Власні middleware: два стилі
//
//   1. RequestTimingMiddleware  — «за конвенцією» (клас без інтерфейсу).
//   2. CorrelationIdMiddleware  — на основі IMiddleware (factory-based, Scoped).
//
// Плюс: інлайн-middleware через app.Use(...) і чому важливий порядок.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// IMiddleware-реалізацію ОБОВ'ЯЗКОВО реєструвати в DI.
// Convention-based (RequestTiming) реєструвати не треба — UseMiddleware<T> сам
// створить екземпляр через ActivatorUtilities.
builder.Services.AddScoped<CorrelationIdMiddleware>();

var app = builder.Build();

// Порядок реєстрації = порядок виконання на вході.
// CorrelationId ставимо раніше — щоб час-логи вже мали id у своєму scope.
app.UseCorrelationId();
app.UseRequestTiming();

// Інлайн-middleware: годиться для дрібниць, які не варто виносити в клас.
app.Use(async (context, next) =>
{
    if (context.Request.Query.ContainsKey("boom"))
        throw new InvalidOperationException("Навмисний виняток для демонстрації (див. приклад 05).");

    await next(context);
});

app.MapGet("/", (HttpContext http) => Results.Ok(new
{
    Message = "Приклад 04. Дивіться заголовки відповіді: X-Correlation-ID, Server-Timing.",
    CorrelationId = http.Items["CorrelationId"],
}));

// Ендпоінт, що штучно «гальмує» — щоб побачити Server-Timing.
app.MapGet("/slow", async () =>
{
    await Task.Delay(Random.Shared.Next(80, 250));
    return Results.Text("Готово (з випадковою затримкою).");
});

app.Run();

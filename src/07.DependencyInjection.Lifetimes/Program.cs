using Common;
using DependencyInjection.Lifetimes;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 07. Час життя сервісів у DI
//
//   Transient  — новий екземпляр на КОЖНЕ впровадження (навіть двічі в одному запиті);
//   Scoped     — один екземпляр на HTTP-запит;
//   Singleton  — один екземпляр на весь застосунок.
//
// Порівнюємо Id тих самих сервісів, узятих у middleware і в endpoint.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddTransient<ITransientOperation, Operation>();
builder.Services.AddScoped<IScopedOperation, Operation>();
builder.Services.AddSingleton<ISingletonOperation, Operation>();
builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();

// Middleware запам'ятовує Id Scoped-сервісу цього запиту.
app.Use(async (context, next) =>
{
    var scoped = context.RequestServices.GetRequiredService<IScopedOperation>();
    context.Items["scopedFromMiddleware"] = scoped.Id;
    await next(context);
});

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", () => "Приклад 07. GET /ids — порівняйте id у межах запиту та між запитами.");

app.MapGet("/ids", (
    HttpContext http,
    ITransientOperation transientA,
    ITransientOperation transientB,
    IScopedOperation scoped,
    ISingletonOperation singleton) => new
{
    transientA = transientA.Id,                       // ← різні між собою
    transientB = transientB.Id,
    scoped = scoped.Id,                               // ← дорівнює scopedFromMiddleware
    scopedFromMiddleware = http.Items["scopedFromMiddleware"],
    singleton = singleton.Id,                         // ← однаковий завжди
});

// Як користуватися Scoped із Singleton / фонового сервісу: створити область вручну.
app.MapGet("/scopes", (IServiceScopeFactory scopeFactory) =>
{
    using var a = scopeFactory.CreateScope();
    using var b = scopeFactory.CreateScope();
    return new
    {
        scopeA = a.ServiceProvider.GetRequiredService<IScopedOperation>().Id,
        scopeB = b.ServiceProvider.GetRequiredService<IScopedOperation>().Id,   // різні області → різні Id
    };
});

app.Run();

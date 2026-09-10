using Common;
using DependencyInjection.Lifetimes;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 07. Час життя сервісів у DI
//
//   Transient  — новий екземпляр на КОЖНЕ впровадження (навіть двічі в одній дії);
//   Scoped     — один екземпляр на HTTP-запит;
//   Singleton  — один екземпляр на весь застосунок.
//
// Порівнюємо Id тих самих сервісів, узятих у middleware і в дії контролера.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddTransient<ITransientOperation, Operation>();
builder.Services.AddScoped<IScopedOperation, Operation>();
builder.Services.AddSingleton<ISingletonOperation, Operation>();
builder.Services.AddControllers();
builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();

// Middleware запам'ятовує Id Scoped-сервісу цього запиту — щоб у дії порівняти,
// що це той самий екземпляр (одна DI-область на весь запит).
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
app.MapControllers();   // дії — у LifetimesController (тека Controllers/)

app.Run();

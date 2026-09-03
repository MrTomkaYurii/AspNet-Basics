using DependencyInjection.Lifetimes;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 07. Час життя сервісів у DI: Transient / Scoped / Singleton
//
//   Transient  — новий екземпляр НА КОЖЕН запит до контейнера (навіть двічі в одному HTTP-запиті).
//   Scoped     — один екземпляр НА HTTP-запит (на область, scope).
//   Singleton  — один екземпляр на весь застосунок.
//
// Порівнюємо Id одних і тих самих сервісів, отримані в різних місцях:
//   • у middleware,
//   • у endpoint напряму,
//   • через проміжний сервіс OperationLogger.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<ITransientOperation, Operation>();
builder.Services.AddScoped<IScopedOperation, Operation>();
builder.Services.AddSingleton<ISingletonOperation, Operation>();
builder.Services.AddScoped<OperationLogger>();

// Хост у Development за замовчуванням валідує області: якщо спробувати
// впровадити Scoped у Singleton — застосунок впаде на старті. Це навмисно.
// (Розкоментуйте, щоб побачити помилку "Cannot consume scoped service ...".)
// builder.Services.AddSingleton<BrokenSingleton>();

var app = builder.Build();

// Middleware бере Scoped-залежності ПАРАМЕТРАМИ InvokeAsync — тут це інлайн,
// і ASP.NET резолвить їх із області поточного запиту.
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<OperationLogger>();
    context.Items["mw"] = logger.Snapshot("middleware");
    await next(context);
});

app.MapGet("/", () => Results.Text(
    "Приклад 07. GET /ids — порівняйте id у межах одного запиту та між запитами."));

// Три способи дістати ті самі сервіси в одному запиті.
app.MapGet("/ids", (
    HttpContext http,
    ITransientOperation t1,
    ITransientOperation t2,          // другий Transient у ТОМУ САМОМУ запиті — інший Id
    IScopedOperation s1,
    IScopedOperation s2,             // другий Scoped — той самий Id
    ISingletonOperation single,
    OperationLogger viaLogger) => Results.Ok(new
{
    directEndpoint = new
    {
        transient_1 = t1.Id,
        transient_2 = t2.Id,         // ← відрізняється від transient_1
        scoped_1 = s1.Id,
        scoped_2 = s2.Id,            // ← дорівнює scoped_1
        singleton = single.Id,
    },
    viaOperationLogger = viaLogger.Snapshot("endpoint→logger"),
    viaMiddleware = http.Items["mw"],
    hint = "У межах запиту: scoped_* і viaMiddleware.scoped збігаються; singleton однаковий ЗАВЖДИ; " +
           "transient різний навіть тут. Перезавантажте — scoped зміниться, singleton ні.",
}));

// Як коректно використати Scoped із Singleton / фонового сервісу: створити область вручну.
app.MapGet("/manual-scope", (IServiceScopeFactory scopeFactory) =>
{
    using var scope1 = scopeFactory.CreateScope();
    var a = scope1.ServiceProvider.GetRequiredService<IScopedOperation>().Id;

    using var scope2 = scopeFactory.CreateScope();
    var b = scope2.ServiceProvider.GetRequiredService<IScopedOperation>().Id;

    return Results.Ok(new { scopeA = a, scopeB = b, note = "різні області → різні Scoped-екземпляри" });
});

app.Run();

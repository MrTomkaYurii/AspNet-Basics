using System.Diagnostics;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 03. Конвеєр обробки запиту «з нуля»
//
// Головна ідея ASP.NET Core:
//
//     запит → [ middleware 1 → middleware 2 → ... → обробник ] → відповідь
//
// Кожен middleware — делегат, що отримує HttpContext і посилання на «наступний»
// (next). Middleware може:
//   • виконати код ДО  await next()  — обробка на вході;
//   • виконати код ПІСЛЯ await next() — обробка на виході (у зворотному порядку);
//   • НЕ викликати next()             — «коротке замикання» (short-circuit).
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
// Цей приклад сервісів не реєструє — уся увага на конвеєрі.

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР  (middleware)
// ======================================================================

// #1 — вимірювання часу. Реєструється ПЕРШИМ → огортає весь конвеєр.
app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();
    context.Response.Headers["X-Request-Id"] = Guid.NewGuid().ToString();

    await next(context);   // ← провалюємось глибше в конвеєр

    app.Logger.LogInformation("{Method} {Path} → {Status} за {Ms} мс",
        context.Request.Method, context.Request.Path,
        context.Response.StatusCode, sw.ElapsedMilliseconds);
});

// #2 — «шлюз». КОРОТКЕ ЗАМИКАННЯ: за невиконаної умови пишемо відповідь самі
// й НЕ викликаємо next() — решта конвеєра не виконається.
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/secret" && context.Request.Headers["X-Api-Key"] != "let-me-in")
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Потрібен заголовок X-Api-Key: let-me-in");
        return;   // ← конвеєр обірвано
    }

    await next(context);
});

// #3 — обробка НА ВИХОДІ: код після next() виконується на шляху відповіді назад.
app.Use(async (context, next) =>
{
    await next(context);

    if (context.Response.StatusCode == StatusCodes.Status404NotFound)
        await context.Response.WriteAsync(" (дописано в middleware #3)");
});

// ======================================================================
//  3 · ОБРОБКА ЗАПИТУ
// ======================================================================
// app.Run(...) — термінальний middleware: next немає. Маршрутизації теж немає,
// шлях розбираємо руками (порівняйте з прикладом 09).
app.Run(async context =>
{
    if (context.Request.Path == "/")
        await context.Response.WriteAsync("Приклад 03. Спробуйте /secret (з заголовком X-Api-Key) та /nope.");
    else if (context.Request.Path == "/secret")
        await context.Response.WriteAsync("Секрет розкрито: конвеєр — це просто список функцій.");
    else
        context.Response.StatusCode = StatusCodes.Status404NotFound;
});

app.Run();   // старт хоста (блокує потік)

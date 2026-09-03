using System.Diagnostics;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 03. Конвеєр обробки запиту «з нуля»
//
// Головна ідея ASP.NET Core:
//
//     запит → [ middleware 1 → middleware 2 → ... → endpoint ] → відповідь
//
// Кожен middleware — це делегат (RequestDelegate), що отримує HttpContext і
// посилання на "наступний" делегат (next). Middleware може:
//   • виконати код ДО next()  — обробка на вході;
//   • викликати await next()  — передати керування далі по конвеєру;
//   • виконати код ПІСЛЯ next() — обробка на виході (стек розкручується назад);
//   • НЕ викликати next()      — "коротке замикання" (short-circuit).
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ── Middleware #1: вимірювання часу ─────────────────────────────────────────
// Реєструється ПЕРШИМ → бачить увесь конвеєр. Код до await next() — на вході,
// код після — на виході (коли всі внутрішні middleware вже відпрацювали).
app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();
    context.Response.Headers["X-Request-Id"] = Guid.NewGuid().ToString("N")[..8];

    await next(context);   // ← провалюємося глибше в конвеєр

    sw.Stop();
    // Увага: заголовки не можна змінювати після того, як почалося тіло відповіді.
    // Тут ми ще встигаємо, бо внутрішні middleware малі. Надійний спосіб —
    // context.Response.OnStarting(...). Про це — приклад 04.
    app.Logger.LogInformation("{Method} {Path} → {Status} за {Elapsed} мс",
        context.Request.Method, context.Request.Path,
        context.Response.StatusCode, sw.ElapsedMilliseconds);
});

// ── Middleware #2: простий "шлюз" за заголовком ─────────────────────────────
// Демонструє КОРОТКЕ ЗАМИКАННЯ: якщо умова не виконана, ми пишемо відповідь
// самі й НЕ викликаємо next() — решта конвеєра (і endpoint) не виконається.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/secret")
        && context.Request.Headers["X-Api-Key"] != "let-me-in")
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { error = "Потрібен коректний X-Api-Key" });
        return;   // ← конвеєр обірвано
    }

    await next(context);
});

// ── Middleware #3: демонстрація обробки "на виході" ─────────────────────────
app.Use(async (context, next) =>
{
    await next(context);

    // Виконується вже ПІСЛЯ endpoint — на шляху відповіді назад.
    if (context.Response.StatusCode == StatusCodes.Status404NotFound
        && !context.Response.HasStarted)
    {
        await context.Response.WriteAsync("\n(додано middleware #3: ресурс не знайдено)");
    }
});

// ── Кінець конвеєра ────────────────────────────────────────────────────────
// app.Run(...) — це "термінальний" middleware: він НІКОЛИ не викликає next.
// Тут ми не використовуємо маршрутизацію взагалі — читаємо шлях руками.
app.Run(async context =>
{
    var path = context.Request.Path.Value ?? "/";

    if (path is "/" or "")
    {
        await context.Response.WriteAsync(
            "Приклад 03. Спробуйте: /hello?name=Іван, /echo (POST), /secret (потрібен заголовок).");
        return;
    }

    if (path == "/hello")
    {
        var name = context.Request.Query["name"].FirstOrDefault() ?? "світ";
        await context.Response.WriteAsync($"Привіт, {name}!");
        return;
    }

    if (path == "/echo" && HttpMethods.IsPost(context.Request.Method))
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.WriteAsync($"Ви надіслали {body.Length} символів:\n{body}");
        return;
    }

    if (path.StartsWith("/secret"))
    {
        await context.Response.WriteAsync("🔓 Секрет розкрито: конвеєр — це просто список функцій.");
        return;
    }

    context.Response.StatusCode = StatusCodes.Status404NotFound;
    await context.Response.WriteAsync("404");
});

app.Run();

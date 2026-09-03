// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 06. Розгалуження конвеєра: Map / MapWhen / UseWhen
//
// Іноді частині запитів потрібен інший набір middleware. Є три інструменти:
//
//   app.Map(path, branch)       — гілка за префіксом шляху. ТЕРМІНАЛЬНА
//                                 (назад у основний конвеєр не повертається).
//                                 Обрізає PathBase: усередині гілки Path вже без префікса.
//
//   app.MapWhen(predicate, br)  — те саме, але умова довільна (не лише шлях).
//                                 Теж термінальна, PathBase НЕ чіпає.
//
//   app.UseWhen(predicate, br)  — умовна ВСТАВКА middleware. Якщо гілка не зробила
//                                 short-circuit, керування ПОВЕРТАЄТЬСЯ в основний конвеєр.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Спільний middleware — виконується для ВСІХ запитів (він перед розгалуженнями).
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Pipeline"] = "main";
    await next(ctx);
});

// ── 1. Map: окрема під-програма на /admin ──────────────────────────────────
app.Map("/admin", admin =>
{
    // Власний конвеєр цієї гілки.
    admin.Use(async (ctx, next) =>
    {
        // Усередині гілки Path вже БЕЗ "/admin".
        if (ctx.Request.Query["token"] != "root")
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Response.WriteAsync("Гілка /admin: потрібен ?token=root");
            return;
        }
        await next(ctx);
    });

    admin.Run(async ctx =>
        await ctx.Response.WriteAsync($"Адмінка. Внутрішній Path = '{ctx.Request.Path}', PathBase = '{ctx.Request.PathBase}'"));
});

// ── 2. MapWhen: гілка за довільною умовою (тут — заголовок) ─────────────────
app.MapWhen(
    ctx => ctx.Request.Headers.ContainsKey("X-Legacy-Client"),
    legacy => legacy.Run(async ctx =>
        await ctx.Response.WriteAsync("Гілка для застарілих клієнтів (визначено за заголовком X-Legacy-Client).")));

// ── 3. UseWhen: умовна вставка, що повертається в основний конвеєр ──────────
app.UseWhen(
    ctx => ctx.Request.Path.StartsWithSegments("/api"),
    api => api.Use(async (ctx, next) =>
    {
        // Перевірка ключа лише для /api/*.
        if (ctx.Request.Headers["X-Api-Key"] != "secret")
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsJsonAsync(new { error = "потрібен X-Api-Key для /api/*" });
            return;   // short-circuit — у основний конвеєр НЕ повертаємось
        }
        ctx.Response.Headers["X-Api-Auth"] = "ok";
        await next(ctx);   // умову виконано → повертаємось у спільні endpoint-и нижче
    }));

// ── Спільні endpoint-и (доступні і для /api/*, бо UseWhen повертає керування) ─
app.MapGet("/", () => "Приклад 06. Спробуйте /admin, /api/ping, заголовок X-Legacy-Client.");
app.MapGet("/api/ping", () => Results.Ok(new { pong = true }));
app.MapGet("/api/whoami", (HttpContext http) => Results.Ok(new { apiAuth = http.Response.Headers["X-Api-Auth"].ToString() }));

app.Run();

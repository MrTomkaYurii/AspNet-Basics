using Common;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 06. Розгалуження конвеєра: Map / MapWhen / UseWhen
//
//   app.Map(path, branch)      — гілка за префіксом шляху. ТЕРМІНАЛЬНА (назад не
//                                повертається). Обрізає префікс: усередині Path без нього.
//   app.MapWhen(pred, branch)  — те саме, але умова довільна. Теж термінальна.
//   app.UseWhen(pred, branch)  — умовна ВСТАВКА. Якщо гілка не зробила short-circuit,
//                                керування ПОВЕРТАЄТЬСЯ в основний конвеєр.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР + РОЗГАЛУЖЕННЯ
// ======================================================================
app.MapApiDocs();

// Map: окрема під-програма на /admin. Усередині гілки Path вже БЕЗ "/admin",
// а PathBase = "/admin".
app.Map("/admin", admin =>
    admin.Run(async ctx =>
        await ctx.Response.WriteAsync($"Адмінка. Path='{ctx.Request.Path}', PathBase='{ctx.Request.PathBase}'")));

// MapWhen: гілка за довільною умовою — тут за заголовком.
app.MapWhen(
    ctx => ctx.Request.Headers.ContainsKey("X-Legacy-Client"),
    legacy => legacy.Run(ctx => ctx.Response.WriteAsync("Гілка для застарілих клієнтів.")));

// UseWhen: middleware лише для /api/*. Якщо ключ є — керування ПОВЕРТАЄТЬСЯ
// в основні endpoint-и нижче; якщо ні — short-circuit.
app.UseWhen(
    ctx => ctx.Request.Path.StartsWithSegments("/api"),
    api => api.Use(async (ctx, next) =>
    {
        if (ctx.Request.Headers["X-Api-Key"] != "secret")
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsync("Потрібен X-Api-Key для /api/*");
            return;
        }
        ctx.Response.Headers["X-Api-Auth"] = "ok";
        await next(ctx);
    }));

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", () => "Приклад 06. Спробуйте /admin, /api/ping, заголовок X-Legacy-Client.");

// /api/* endpoint-и доступні, бо UseWhen повернув керування сюди.
app.MapGet("/api/ping", () => "pong");
app.MapGet("/api/whoami", (HttpContext http) =>
    $"X-Api-Auth = {http.Response.Headers["X-Api-Auth"]}");

app.Run();

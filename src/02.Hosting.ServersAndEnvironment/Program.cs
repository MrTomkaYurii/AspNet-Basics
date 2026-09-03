using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 02. HTTP-сервери та середовища виконання
//
// Дві теми:
//   1. Який сервер обробляє запити (Kestrel / IIS / HTTP.sys) і як його налаштувати.
//   2. Що таке "середовище" (Development / Staging / Production) і як код на нього реагує.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ── Налаштування Kestrel ────────────────────────────────────────────────────
// Kestrel — крос-платформенний HTTP-сервер за замовчуванням. У проді його
// зазвичай ставлять ЗА реверс-проксі (Nginx, IIS, YARP), але він уміє й сам
// «дивитися в інтернет».
//
// Тут читаємо секцію "Kestrel" з appsettings.json І додаємо ліміти з коду.
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Configure(context.Configuration.GetSection("Kestrel"));

    // Захисні ліміти (значення навмисно занижені для наочності).
    options.Limits.MaxRequestBodySize = 1 * 1024 * 1024;          // 1 МБ
    options.Limits.MaxConcurrentConnections = 100;
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(120);
    options.AddServerHeader = false;                              // не світити "Server: Kestrel"
});

var app = builder.Build();

// IWebHostEnvironment.Environment* — головний спосіб «розгалужувати» поведінку.
// Значення береться зі змінної ASPNETCORE_ENVIRONMENT (див. launchSettings.json).
var env = app.Environment;

// Класичний приклад: детальна сторінка помилки лише в Development.
if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // У проді — узагальнена відповідь без деталей стека.
    app.UseExceptionHandler(b => b.Run(async ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await ctx.Response.WriteAsync("Внутрішня помилка сервера.");
    }));
}

app.MapGet("/", () => "Приклад 02: сервери та середовища. Див. /env, /server, /config.");

// Хто ми і де ми.
app.MapGet("/env", (IWebHostEnvironment e) => Results.Ok(new
{
    e.EnvironmentName,
    IsDevelopment = e.IsDevelopment(),
    IsStaging = e.IsStaging(),
    IsProduction = e.IsProduction(),
    IsCustomEnv = e.IsEnvironment("Testing"),
    e.ApplicationName,
    e.ContentRootPath,
    e.WebRootPath,
}));

// На яких адресах реально слухає сервер (може відрізнятися від бажаного).
app.MapGet("/server", (IServer server) =>
{
    var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses ?? [];
    return Results.Ok(new
    {
        Server = server.GetType().Name,           // KestrelServer / IISHttpServer / ...
        ListeningOn = addresses,
    });
});

// Показуємо злиття конфігурації: значення нижче різне для Development і Production
// (див. appsettings.json vs appsettings.Production.json).
app.MapGet("/config", (IConfiguration config) => Results.Ok(new
{
    FeatureFlag = config.GetValue<bool>("Features:ExperimentalSearch"),
    KestrelHttpUrl = config["Kestrel:Endpoints:Http:Url"],
    ContentRoot = config["contentRoot"],
}));

// Ендпоінт, що існує ЛИШЕ в Development — типовий прийом для діагностики.
if (env.IsDevelopment())
{
    app.MapGet("/dev/routes", (IEnumerable<EndpointDataSource> sources) =>
        sources.SelectMany(s => s.Endpoints).Select(e => e.DisplayName));
}

app.Run();

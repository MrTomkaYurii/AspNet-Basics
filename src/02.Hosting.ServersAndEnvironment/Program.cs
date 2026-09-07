using Common;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Scalar.AspNetCore;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 02. HTTP-сервери та середовища виконання
//
//   1. Який сервер обробляє запити (Kestrel / IIS / HTTP.sys) і як його налаштувати.
//   2. Що таке «середовище» (Development / Production) і як код на нього реагує.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ  (реєстрація в DI + налаштування хоста)
// ======================================================================

// Kestrel — крос-платформенний HTTP-сервер за замовчуванням. Ось так його
// налаштовують (тут — прибираємо заголовок "Server: Kestrel" і обмежуємо тіло запиту).
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxRequestBodySize = 1 * 1024 * 1024;   // 1 МБ
});

// Документація підключається НАПРЯМУ, без спільного помічника Common.ApiDocs —
// щоб було видно самі виклики (детально розібрано в прикладі 01):
//   • AddOpenApi()  — генератор документа /openapi/v1.json;
//   • MapScalarApiReference() нижче — переглядач цього документа на /scalar.
// builder.Services.AddApiDocs();   // ← замінено прямим викликом
builder.Services.AddOpenApi();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР  (middleware)
// ======================================================================
// app.MapApiDocs();   // ← замінено секцією нижче
app.MapOpenApi();                                                    // GET /openapi/v1.json
app.MapScalarApiReference(o => o.WithOpenApiRoutePattern("/openapi/v1.json"));  // GET /scalar

// Класична реакція на середовище: детальна сторінка помилки лише в Development.
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/error");

// ======================================================================
//  3 · ЗАПИТИ  (endpoints)
// ======================================================================
app.MapGet("/", () => "Приклад 02. Див. /env, /server, /config.");

// Хто ми і де ми.
app.MapGet("/env", (IWebHostEnvironment env) => new
{
    env.EnvironmentName,
    IsDevelopment = env.IsDevelopment(),
    env.ApplicationName,
});

// Який сервер і на яких адресах реально слухає.
app.MapGet("/server", (IServer server) => new
{
    Server = server.GetType().Name,   // KestrelServer / IISHttpServer / ...
    Addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses,
});

// Значення з конфігурації — різне для Development і Production
// (appsettings.json проти appsettings.Production.json).
app.MapGet("/config", (IConfiguration config) => new
{
    ExperimentalSearch = config.GetValue<bool>("Features:ExperimentalSearch"),
});

// Ендпоінт лише для Development — типовий прийом для діагностики.
if (app.Environment.IsDevelopment())
    app.MapGet("/dev/ping", () => "pong (тільки в Development)");

app.MapGet("/error", () => Results.Problem("Внутрішня помилка сервера."));

app.Run();

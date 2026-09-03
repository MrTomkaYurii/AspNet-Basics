using Common;
using Common.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 01. Мінімальний хост ASP.NET Core
//
// Мета: побачити, з чого складається веб-застосунок на .NET. Він має три частини:
//
//   1) Host        — керує запуском/зупинкою, містить DI, конфігурацію, логування;
//   2) HTTP-сервер — Kestrel, приймає запити і формує HttpContext;
//   3) Конвеєр     — послідовність middleware, що обробляє кожен запит.
// ─────────────────────────────────────────────────────────────────────────────

// CreateBuilder уже налаштував конфігурацію, логування, Kestrel і DI-контейнер.
var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ  (реєстрація в DI)
// ======================================================================
// Доки не викликано Build(), ми лише НАПОВНЮЄМО контейнер.
builder.Services.AddCatalog();
builder.Services.AddApiDocs();   // /openapi, /swagger, /scalar

// Build() «запечатує» контейнер: далі нові сервіси додавати не можна.
var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР  (middleware)
// ======================================================================
app.MapApiDocs();

// Хуки життєвого циклу процесу. На Ctrl+C хост перестає приймати нові запити,
// дає час добити поточні, і лише потім завершується — це graceful shutdown.
app.Lifetime.ApplicationStarted.Register(() => app.Logger.LogInformation("Застосунок запущено"));
app.Lifetime.ApplicationStopping.Register(() => app.Logger.LogInformation("Зупинка: завершуємо поточні запити…"));
app.Lifetime.ApplicationStopped.Register(() => app.Logger.LogInformation("Застосунок зупинено"));

// ======================================================================
//  3 · ЗАПИТИ  (endpoints)
// ======================================================================
app.MapGet("/", () => "Привіт із мінімального хоста!");

// Сервіси з DI просто «просяться» в параметри хендлера.
app.MapGet("/env", (IWebHostEnvironment env) => new
{
    env.ApplicationName,
    env.EnvironmentName,
});

// ICatalog теж резолвиться з контейнера — доказ, що AddCatalog() спрацював.
app.MapGet("/products", (ICatalog catalog) => catalog.GetProducts());

// Керована зупинка ззовні.
app.MapGet("/shutdown", (IHostApplicationLifetime life) =>
{
    life.StopApplication();
    return "Зупиняюсь…";
});

// app.Run() блокує потік і віддає керування хосту до сигналу зупинки.
app.Run();

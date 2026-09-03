using Common;
using Common.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 01. Мінімальний хост ASP.NET Core
//
// Мета: побачити, з чого взагалі складається веб-застосунок на .NET, ще до
// будь-яких контролерів і маршрутів. Весь застосунок — це:
//
//   1) Host        — контейнер, що керує запуском/зупинкою, DI, конфігурацією,
//                    логуванням і фоновими сервісами;
//   2) HTTP-сервер — Kestrel, що приймає TCP-з'єднання і формує HttpContext;
//   3) Конвеєр     — послідовність middleware, що обробляє кожен запит.
// ─────────────────────────────────────────────────────────────────────────────

// WebApplication.CreateBuilder робить ДУЖЕ багато за замовчуванням:
//  • читає конфігурацію з appsettings.json, appsettings.{Environment}.json,
//    змінних середовища та аргументів командного рядка;
//  • налаштовує логування (Console, Debug, EventSource);
//  • реєструє Kestrel як HTTP-сервер;
//  • створює DI-контейнер (IServiceCollection).
var builder = WebApplication.CreateBuilder(args);

// ── ЕТАП 1: реєстрація сервісів у DI-контейнері ──────────────────────────────
// Доки не викликано builder.Build(), ми лише НАПОВНЮЄМО контейнер.
builder.Services.AddCatalog();

// builder.Configuration — уже готовий IConfiguration. Значення нижче можна
// перевизначити у appsettings.json або змінною середовища Greeting__Message.
var greeting = builder.Configuration.GetValue("Greeting:Message", "Привіт із мінімального хоста!");

// builder.Logging — точка налаштування провайдерів логування.
builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Information);

// ── ЕТАП 2: побудова застосунку ─────────────────────────────────────────────
// Build() «запечатує» DI-контейнер: після цього нові сервіси додавати не можна.
var app = builder.Build();

// IHostApplicationLifetime дає доступ до подій життєвого циклу процесу.
// Саме тут видно "graceful shutdown": на Ctrl+C хост спершу перестає приймати
// нові запити, дає час завершити поточні, і лише потім зупиняється.
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

lifetime.ApplicationStarted.Register(() => logger.LogInformation("✅ Застосунок запущено"));
lifetime.ApplicationStopping.Register(() => logger.LogInformation("⏳ Отримано сигнал зупинки, завершуємо поточні запити…"));
lifetime.ApplicationStopped.Register(() => logger.LogInformation("🛑 Застосунок зупинено"));

// ── ЕТАП 3: конвеєр обробки запитів ─────────────────────────────────────────
// app.MapGet — це вже endpoint. Детально маршрутизацію розбираємо в прикладі 09.

app.MapGet("/", () => Results.Text(greeting));

// Показуємо, що ILogger і сервіси з DI просто "просяться" в параметри хендлера.
app.MapGet("/info", (IWebHostEnvironment env, ILogger<Program> log) =>
{
    log.LogInformation("Запит інформації про середовище");
    return Results.Ok(new
    {
        env.ApplicationName,
        env.EnvironmentName,
        env.ContentRootPath,
        Framework = Environment.Version.ToString(),
        ProcessId = Environment.ProcessId,
    });
});

// ICatalog також резолвиться з контейнера — доводимо, що AddCatalog() спрацював.
app.MapGet("/products", (ICatalog catalog) => catalog.GetProducts());

// Демонстрація керованої зупинки: GET /shutdown ініціює graceful shutdown.
app.MapGet("/shutdown", (IHostApplicationLifetime life) =>
{
    life.StopApplication();
    return Results.Text("Ініційовано зупинку хоста.");
});

// app.Run() — блокувальний виклик: стартує Kestrel і віддає керування хосту
// до отримання сигналу зупинки (Ctrl+C, SIGTERM, StopApplication()).
app.Run();

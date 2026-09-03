using Configuration.Options;
using Microsoft.Extensions.Options;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 08. Конфігурація та патерн Options
//
// Дві теми:
//   1. Звідки береться конфігурація і хто кого перекриває.
//   2. Як перетворити рядкові ключі на типізований об'єкт і які є способи
//      його "доставки": IOptions / IOptionsSnapshot / IOptionsMonitor.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ── Джерела конфігурації (додаються CreateBuilder у такому порядку; кожне
//    наступне ПЕРЕКРИВАЄ попередні за однаковим ключем):
//      1. appsettings.json
//      2. appsettings.{Environment}.json
//      3. User Secrets            (лише Development)
//      4. змінні середовища       (Catalog__Currency=EUR)
//      5. аргументи CLI           (--Catalog:Currency=EUR)
//
// Можна додати власне джерело:
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Catalog:PoweredBy"] = "AspNet-Basics",   // цього ключа немає у файлах
});

// ── Прив'язка секції до класу + валідація ──────────────────────────────────
builder.Services
    .AddOptions<CatalogOptions>()
    .Bind(builder.Configuration.GetSection(CatalogOptions.SectionName))
    .ValidateDataAnnotations()          // перевіряє [Required], [Range], [RegularExpression]
    .ValidateOnStart();                 // падіння на СТАРТІ, а не при першому зверненні

var app = builder.Build();

app.MapGet("/", () => Results.Text(
    "Приклад 08. /config/sources, /config/value, /options/io, /options/snapshot, /options/monitor"));

// Показуємо ланцюжок провайдерів конфігурації.
app.MapGet("/config/sources", (IConfiguration config) =>
{
    var root = (IConfigurationRoot)config;
    return Results.Ok(new
    {
        providers = root.Providers.Select(p => p.ToString()).ToArray(),
        debugView = root.GetDebugView(),      // повне дерево ключ→значення+джерело
    });
});

// Прямий доступ до значення (без Options) — інколи доречно (одноразове читання на старті).
app.MapGet("/config/value", (IConfiguration config) => Results.Ok(new
{
    storeName = config["Catalog:StoreName"],
    poweredBy = config["Catalog:PoweredBy"],       // з InMemory-джерела
    nested = config.GetValue<int>("Catalog:DefaultPageSize"),
}));

// IOptions<T> — SINGLETON. Значення обчислюється один раз і не змінюється.
// Годиться для більшості випадків.
app.MapGet("/options/io", (IOptions<CatalogOptions> opt) => Results.Ok(opt.Value));

// IOptionsSnapshot<T> — SCOPED. Перечитується раз на запит: зміни в
// appsettings.json підхопляться на наступному запиті. У middleware/singleton не інжектиться.
app.MapGet("/options/snapshot", (IOptionsSnapshot<CatalogOptions> opt) => Results.Ok(new
{
    opt.Value,
    note = "змініть appsettings.json під час роботи й повторіть запит — значення оновиться",
}));

// IOptionsMonitor<T> — SINGLETON, але з CurrentValue та підпискою OnChange.
// Єдиний варіант «живих» опцій для middleware та фонових сервісів.
app.MapGet("/options/monitor", (IOptionsMonitor<CatalogOptions> mon) => Results.Ok(new
{
    current = mon.CurrentValue,
    note = "IOptionsMonitor.OnChange(...) дає колбек на кожну зміну файлу",
}));

app.Run();

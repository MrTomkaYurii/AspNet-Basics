using Common;
using Configuration.Options;
using Microsoft.Extensions.Options;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 08. Конфігурація та патерн Options
//
//   1. Звідки береться конфігурація і хто кого перекриває.
//   2. Три способи «доставки» типізованих опцій: IOptions / IOptionsSnapshot / IOptionsMonitor.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================

// Джерела конфігурації (CreateBuilder додає в такому порядку; кожне наступне
// перекриває попередні): appsettings.json → appsettings.{Environment}.json →
// User Secrets (Dev) → змінні середовища → аргументи CLI.

// Прив'язка секції "Catalog" до класу + валідація.
builder.Services
    .AddOptions<CatalogOptions>()
    .Bind(builder.Configuration.GetSection(CatalogOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();          // падіння на СТАРТІ, а не при першому зверненні

builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", () => "Приклад 08. /sources, /options/io, /options/snapshot, /options/monitor");

// Повне дерево «ключ → значення (+ джерело)» — зручно для діагностики.
app.MapGet("/sources", (IConfiguration config) =>
    Results.Text(((IConfigurationRoot)config).GetDebugView()));

// IOptions<T> — Singleton. Обчислюється один раз. Підходить для більшості випадків.
app.MapGet("/options/io", (IOptions<CatalogOptions> opt) => opt.Value);

// IOptionsSnapshot<T> — Scoped. Перечитується раз на запит: зміни в appsettings.json
// підхопляться на наступному запиті. У middleware / singleton не інжектиться.
app.MapGet("/options/snapshot", (IOptionsSnapshot<CatalogOptions> opt) => opt.Value);

// IOptionsMonitor<T> — Singleton із CurrentValue та підпискою OnChange.
// Єдиний варіант «живих» опцій для middleware і фонових сервісів.
app.MapGet("/options/monitor", (IOptionsMonitor<CatalogOptions> mon) => mon.CurrentValue);

app.Run();

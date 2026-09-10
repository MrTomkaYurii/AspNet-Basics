using Common;
using Configuration.Options;

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

builder.Services.AddControllers();
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
app.MapControllers();   // дії — у OptionsController (тека Controllers/)

app.Run();

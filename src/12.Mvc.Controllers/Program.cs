// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 12. MVC-контролери для Web API
//
// Альтернатива Minimal API: контролери-класи. Доречні, коли багато endpoint-ів
// зі спільною поведінкою, потрібні фільтри, конвенції, успадкування, area-и.
//
// Той самий конвеєр, той самий DI — інша «форма» оголошення endpoint-ів.
// ─────────────────────────────────────────────────────────────────────────────

using Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCatalog();

// Реєструє інфраструктуру MVC: активацію контролерів, model binding,
// форматери (System.Text.Json), ApiExplorer тощо.
builder.Services.AddControllers();

// Налаштування поведінки [ApiController] — напр. лишити стандартну відповідь 400.
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(o =>
{
    o.SuppressModelStateInvalidFilter = false; // (за замовчуванням) авто-400 при невалідній моделі
});

var app = builder.Build();

app.MapGet("/", () => Results.Redirect("/api/products"));

// Додає endpoint-и, знайдені в контролерах, до таблиці маршрутів.
app.MapControllers();

app.Run();

using Common;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 05. Наскрізна функціональність (cross-cutting concerns)
//
// Речі, потрібні майже кожному застосунку, — усі реалізовані як middleware:
//   • обробка необроблених винятків → Problem Details (RFC 9457);
//   • тіло відповіді для «голих» статус-кодів (404 тощо);
//   • статичні файли з wwwroot/;
//   • CORS — запити з іншого origin у браузері.
//
// Головний урок: ПОРЯДОК реєстрації middleware критичний.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddProblemDetails();   // єдиний формат помилок для 4xx/5xx

builder.Services.AddCors(options =>
    options.AddPolicy("demo", policy => policy
        .WithOrigins("https://example.com")
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР — ПОРЯДОК КРИТИЧНИЙ
// ======================================================================

// 1. Обробка винятків — НАЙПЕРША, щоб огорнути все нижче.
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler();          // порожній виклик → Problem Details

// 2. Тіло для «голих» статус-кодів (напр. 404 без тіла).
app.UseStatusCodePages();

// 3. Статичні файли — ДО маршрутизації: якщо файл є, конвеєр замикається тут.
app.UseDefaultFiles();                  // "/" → "/index.html"
app.UseStaticFiles();

// 4. CORS — після UseRouting (неявний), до endpoint-ів.
app.UseCors("demo");

app.MapApiDocs();

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================

// Успішна відповідь (політика CORS "demo" застосовується до всіх endpoint-ів).
app.MapGet("/api/data", () => new { value = 42 });

// Кидає виняток → у Production стане 500 Problem Details.
app.MapGet("/api/fail", void () => throw new InvalidOperationException("Щось пішло не так."));

// Явний 404 → UseStatusCodePages додасть тіло.
app.MapGet("/api/missing", () => Results.NotFound());

app.Run();

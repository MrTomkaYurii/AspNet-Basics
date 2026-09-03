// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 05. Наскрізна функціональність (cross-cutting concerns)
//
// Речі, які потрібні майже кожному застосунку і які реалізовано як middleware:
//   • обробка необроблених винятків → відповідь у форматі Problem Details (RFC 9457);
//   • відповіді за статус-кодами без тіла (404, 401 …);
//   • статичні файли (wwwroot);
//   • CORS (cross-origin requests із браузера).
//
// І головний урок модуля: ПОРЯДОК реєстрації middleware критичний.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// AddProblemDetails() навчає фреймворк формувати єдиний формат помилок
// (application/problem+json) для 4xx/5xx, які згенерував сам фреймворк.
builder.Services.AddProblemDetails(options =>
{
    // Додаємо кастомні поля до кожної проблеми.
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Instance = ctx.HttpContext.Request.Path;
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("demo", policy => policy
        .WithOrigins("https://example.com", "http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// ── 1. ОБРОБКА ВИНЯТКІВ — має бути НАЙПЕРШОЮ ────────────────────────────────
// Тільки так вона «огорне» всі middleware нижче. У Development показуємо
// сторінку розробника, інакше — Problem Details.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();      // порожній виклик → використовує ProblemDetails
}

// 2. Відповіді за статус-кодами (напр. згенерувати тіло для «голого» 404).
app.UseStatusCodePages();

// 3. Статичні файли з wwwroot/. Ставимо ДО маршрутизації: якщо файл існує,
//    конвеєр коротко замикається і до endpoint-ів справа не доходить.
app.UseStaticFiles();

// 4. CORS. Має бути ПІСЛЯ UseRouting (тут неявний) і ДО endpoint-ів.
app.UseCors("demo");

// ── Endpoint-и ─────────────────────────────────────────────────────────────
app.MapGet("/api/data", () => Results.Ok(new { value = 42, note = "цей ресурс доступний з CORS-політикою 'demo'" }))
   .RequireCors("demo");

// Кидає виняток → UseExceptionHandler перетворить на 500 Problem Details.
app.MapGet("/api/fail", void () => throw new InvalidOperationException("Щось пішло не так усередині обробника."));

// Явно повертаємо 404 → UseStatusCodePages додасть тіло.
app.MapGet("/api/missing", () => Results.NotFound());

// Повертаємо Problem Details вручну з корисними полями.
app.MapGet("/api/teapot", () => Results.Problem(
    title: "Я чайник",
    detail: "Цей ресурс відмовляється варити каву.",
    statusCode: StatusCodes.Status418ImATeapot));

app.Run();

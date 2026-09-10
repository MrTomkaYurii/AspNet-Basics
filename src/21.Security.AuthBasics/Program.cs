using System.Text;
using Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 21. Основи безпеки: автентифікація та авторизація
//
//   Автентифікація (AuthN) — ХТО це? Будує ClaimsPrincipal (HttpContext.User).
//   Авторизація   (AuthZ) — ЧИ МОЖНА? Перевіряє права цієї особи.
//
// Обидві — middleware. Порядок: UseAuthentication → UseAuthorization → endpoints.
// На контролерах вимоги вішають атрибутом [Authorize] (з політикою або без).
//
// УВАГА: користувачі й ключ підпису ЗАШИТІ в код — це навчальний приклад.
// У проді: справжній Identity Provider, ключ із секретів, обов'язковий HTTPS.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
var signingKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes("ДЕМО-КЛЮЧ-НЕ-ДЛЯ-ПРОДУ-мінімум-32-байти!!"));

// Ключ потрібен і для валідації (тут), і для видачі токена (AuthController).
builder.Services.AddSingleton(signingKey);

// AuthN: схема "Bearer" — перевірка підпису та терміну дії JWT.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateLifetime = true,
        ValidateIssuer = false,
        ValidateAudience = false,
    });

// AuthZ: одна іменована політика (роль admin). "Будь-хто автентифікований" —
// це просто [Authorize] без політики.
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("admin", p => p.RequireRole("admin"));

builder.Services.AddControllers();
builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР — ПОРЯДОК КРИТИЧНИЙ
// ======================================================================
app.MapApiDocs();
app.UseAuthentication();   // читає Authorization: Bearer <jwt>, будує HttpContext.User
app.UseAuthorization();    // застосовує [Authorize]

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", () => "Приклад 21. POST /token (alice або bob, пароль 'password'), далі /me та /admin з Bearer.");
app.MapControllers();

app.Run();

using System.Security.Claims;
using System.Text;
using Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 21. Основи безпеки: автентифікація та авторизація
//
//   Автентифікація (AuthN) — ХТО це? Будує ClaimsPrincipal (HttpContext.User).
//   Авторизація   (AuthZ) — ЧИ МОЖНА? Перевіряє права цієї особи.
//
// Обидві — middleware. Порядок: UseAuthentication → UseAuthorization → endpoints.
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
// це просто .RequireAuthorization() без політики.
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("admin", p => p.RequireRole("admin"));

builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР — ПОРЯДОК КРИТИЧНИЙ
// ======================================================================
app.MapApiDocs();
app.UseAuthentication();   // читає Authorization: Bearer <jwt>, будує HttpContext.User
app.UseAuthorization();    // застосовує [Authorize] / RequireAuthorization

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================
app.MapGet("/", () => "Приклад 21. POST /token (alice або bob, пароль 'password'), далі /me та /admin з Bearer.");

// Видача токена (у реальності — окремий Identity Provider).
app.MapPost("/token", (LoginRequest login) =>
{
    string? role = login switch
    {
        { Username: "alice", Password: "password" } => "admin",
        { Username: "bob", Password: "password" } => "user",
        _ => null,
    };
    if (role is null)
        return Results.Unauthorized();

    var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, login.Username),
            new Claim(ClaimTypes.Role, role),
        ]),
        Expires = DateTime.UtcNow.AddMinutes(30),
        SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
    });
    return Results.Ok(new { access_token = token });
});

app.MapGet("/public", () => "Відкритий ресурс — токен не потрібен.");

// Будь-який автентифікований користувач.
app.MapGet("/me", (ClaimsPrincipal user) =>
        new { name = user.Identity?.Name, role = user.FindFirstValue(ClaimTypes.Role) })
   .RequireAuthorization();

// Потрібна роль admin.
app.MapGet("/admin", () => "Вітаю в адмінці.")
   .RequireAuthorization("admin");

app.Run();

public sealed record LoginRequest(string Username, string Password);

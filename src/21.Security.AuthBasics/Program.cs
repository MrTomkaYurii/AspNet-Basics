using System.Security.Claims;
using System.Text;
using Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 21. Основи безпеки: автентифікація та авторизація
//
//   Автентифікація (AuthN) — ХТО це? Встановлює особу (ClaimsPrincipal).
//   Авторизація   (AuthZ) — ЧИ МОЖНА? Перевіряє права встановленої особи.
//
// Обидві — middleware. Порядок: UseRouting → UseAuthentication → UseAuthorization → endpoints.
//
// УВАГА: користувачі й ключ підпису тут ЗАШИТІ в код — це навчальний приклад.
// У проді: справжнє сховище користувачів, ключ із секретів, HTTPS обов'язково.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCatalog();

const string issuer = "aspnet-basics";
const string audience = "aspnet-basics-clients";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
    "ЦЕ-ДЕМО-КЛЮЧ-НЕ-ВИКОРИСТОВУЙТЕ-В-ПРОДІ-мінімум-32-байти!!"));

// ── AuthN: схема "Bearer" з перевіркою JWT ────────────────────────────────
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(5),
        };
    });

// ── AuthZ: політики ──────────────────────────────────────────────────────
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("admin-only", p => p.RequireRole("admin"))
    .AddPolicy("premium", p => p.RequireClaim("subscription", "premium"))
    .AddPolicy("adult", p => p.RequireAssertion(ctx =>
        ctx.User.HasClaim(c => c.Type == "age") &&
        int.TryParse(ctx.User.FindFirstValue("age"), out var age) && age >= 18));

var app = builder.Build();

// Порядок критичний.
app.UseAuthentication();   // читає заголовок Authorization, будує HttpContext.User
app.UseAuthorization();    // застосовує [Authorize] / RequireAuthorization до обраного endpoint

// ── Видача токена (у реальності — окремий Identity Provider) ──────────────
app.MapPost("/token", (LoginRequest login) =>
{
    // Демо-сховище користувачів.
    (string Role, string? Subscription, int Age)? user = login switch
    {
        { Username: "alice", Password: "password" } => ("admin", "premium", 30),
        { Username: "bob", Password: "password" } => ("user", null, 16),
        _ => null,
    };
    if (user is null) return Results.Unauthorized();

    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, login.Username),
        new(ClaimTypes.Name, login.Username),
        new(ClaimTypes.Role, user.Value.Role),
        new("age", user.Value.Age.ToString()),
    };
    if (user.Value.Subscription is { } sub)
        claims.Add(new Claim("subscription", sub));

    var descriptor = new SecurityTokenDescriptor
    {
        Issuer = issuer,
        Audience = audience,
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddMinutes(30),
        SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
    };
    var token = new JsonWebTokenHandler().CreateToken(descriptor);
    return Results.Ok(new { access_token = token, token_type = "Bearer", expires_in = 1800 });
});

// ── Ендпоінти з різними вимогами ─────────────────────────────────────────

app.MapGet("/public", () => "Відкритий ресурс — токен не потрібен.");

// Будь-який автентифікований користувач.
app.MapGet("/me", (ClaimsPrincipal user) => Results.Ok(new
{
    name = user.Identity?.Name,
    isAuthenticated = user.Identity?.IsAuthenticated,
    claims = user.Claims.Select(c => new { c.Type, c.Value }),
}))
.RequireAuthorization();

// Потрібна роль admin.
app.MapGet("/admin", () => "Вітаю в адмінці.")
   .RequireAuthorization("admin-only");

// Потрібен claim subscription=premium.
app.MapGet("/premium", () => "Преміум-контент.")
   .RequireAuthorization("premium");

// Кастомна політика: age >= 18.
app.MapGet("/adult", () => "18+ контент.")
   .RequireAuthorization("adult");

app.MapGet("/", () => Results.Text(
    "Приклад 21. POST /token (alice/password або bob/password), далі /me, /admin, /premium, /adult з Bearer."));

app.Run();

public sealed record LoginRequest(string Username, string Password);

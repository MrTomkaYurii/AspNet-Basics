using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Security.AuthBasics.Controllers;

public sealed record LoginRequest(string Username, string Password);

/// <summary>Видача токена (у реальності — окремий Identity Provider).</summary>
[ApiController]
[AllowAnonymous]
public sealed class AuthController(SymmetricSecurityKey signingKey) : ControllerBase
{
    [HttpPost("/token")]
    public IActionResult Token(LoginRequest login)
    {
        string? role = login switch
        {
            { Username: "alice", Password: "password" } => "admin",
            { Username: "bob", Password: "password" } => "user",
            _ => null,
        };
        if (role is null)
            return Unauthorized();

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
        return Ok(new { access_token = token });
    }

    [HttpGet("/public")]
    public string Public() => "Відкритий ресурс — токен не потрібен.";
}

/// <summary>Будь-який автентифікований користувач: <c>[Authorize]</c> без політики.</summary>
[ApiController]
[Authorize]
public sealed class MeController : ControllerBase
{
    [HttpGet("/me")]
    public object Me()
        => new { name = User.Identity?.Name, role = User.FindFirstValue(ClaimTypes.Role) };
}

/// <summary>Потрібна роль admin: <c>[Authorize(Policy = "admin")]</c>.</summary>
[ApiController]
[Authorize(Policy = "admin")]
public sealed class AdminController : ControllerBase
{
    [HttpGet("/admin")]
    public string Index() => "Вітаю в адмінці.";
}

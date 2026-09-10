using Microsoft.AspNetCore.Mvc;

namespace Mvc.BindingAndValidation.Controllers;

/// <summary>
/// ЧАСТИНА 1. Джерела прив'язки в MVC.
///
/// Правила за замовчуванням (з <c>[ApiController]</c>):
///   • ім'я параметра = сегмент маршруту  → route values;
///   • простий тип, не в маршруті          → query string;
///   • складний тип                        → тіло запиту (JSON);
///   • тип зареєстровано в DI              → з контейнера.
/// Явні атрибути <c>[FromRoute]/[FromQuery]/[FromHeader]/[FromBody]/[FromServices]</c>
/// перекривають вивід.
/// </summary>
[ApiController]
[Route("bind")]
public sealed class BindController : ControllerBase
{
    // route values: ім'я = сегмент {id}
    [HttpGet("route/{id:int}")]
    public object FromRoute(int id) => new { from = "route", id };

    // query, зокрема масив (?tags=a&tags=b). q — nullable → необов'язковий;
    // page має значення за замовчуванням; non-nullable без default був би обов'язковим.
    [HttpGet("query")]
    public object FromQuery([FromQuery] string? q, [FromQuery] string[] tags, [FromQuery] int page = 1)
        => new { from = "query", q, tags, page };

    // заголовок — явно через атрибут. Non-nullable → обов'язковий (немає → авто-400).
    [HttpGet("header")]
    public object FromHeader([FromHeader(Name = "X-Tenant")] string tenant)
        => new { from = "header", tenant };

    // Аналог Minimal API [AsParameters]: page з маршруту, term/pageSize з query.
    // У MVC перелічуємо параметри або робимо модель з атрибутами джерел на властивостях.
    [HttpGet("search/{page:int}")]
    public object Search([FromRoute] int page, [FromQuery] string? term, [FromQuery] int pageSize = 20)
        => new { page, term, pageSize };
}

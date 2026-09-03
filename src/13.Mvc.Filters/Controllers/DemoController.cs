using Microsoft.AspNetCore.Mvc;
using Mvc.Filters.Filters;

namespace Mvc.Filters.Controllers;

[ApiController]
[Route("api/[controller]")]
// Фільтри рівня контролера — застосовуються до всіх дій.
// [ServiceFilter] → екземпляр береться з DI.
[ServiceFilter(typeof(DemoResourceFilter))]
[ServiceFilter(typeof(DemoActionFilter))]
[ServiceFilter(typeof(DemoResultFilter))]
public sealed class DemoController : ControllerBase
{
    // Звичайний потік: у відповіді буде поле "steps" — порядок фільтрів.
    [HttpGet("ok")]
    public IActionResult Ok200() => Ok(new { message = "усе добре" });

    // Виняток → перехопить ExceptionFilter (нижче, рівня цієї дії).
    [HttpGet("boom")]
    [ServiceFilter(typeof(DemoExceptionFilter))]
    public IActionResult Boom() => throw new InvalidOperationException("Навмисна помилка.");
}

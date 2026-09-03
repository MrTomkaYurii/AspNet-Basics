using Microsoft.AspNetCore.Mvc;
using Mvc.Filters.Filters;

namespace Mvc.Filters.Controllers;

[ApiController]
[Route("api/[controller]")]
// Фільтри рівня контролера застосовуються до всіх дій.
// ServiceFilter → екземпляр береться з DI (можна мати залежності).
[ServiceFilter(typeof(TimingResourceFilter))]
[ServiceFilter(typeof(LoggingActionFilter))]
[ServiceFilter(typeof(EnvelopeResultFilter))]
public sealed class DemoController : ControllerBase
{
    // Звичайна дія: у відповіді буде «конверт» із полем trace — порядок фільтрів.
    [HttpGet("ok")]
    public IActionResult Ok200()
    {
        HttpContext.Items.TryGetValue("trace", out var t);
        ((List<string>)t!).Add("4: Action body");
        return Ok(new { message = "усе добре" });
    }

    // Дія кидає виняток → його перехопить ExceptionFilter (нижче, рівня дії).
    [HttpGet("boom")]
    [ServiceFilter(typeof(DemoExceptionFilter))]
    public IActionResult Boom()
    {
        HttpContext.Items.TryGetValue("trace", out var t);
        ((List<string>)t!).Add("4: Action body (кидає виняток)");
        throw new InvalidOperationException("Навмисна помилка в дії.");
    }

    // Дія зі штучною затримкою — подивіться заголовок X-Elapsed-Ms від ResourceFilter.
    [HttpGet("slow")]
    public async Task<IActionResult> Slow()
    {
        await Task.Delay(150);
        return Ok(new { message = "повільна відповідь" });
    }
}

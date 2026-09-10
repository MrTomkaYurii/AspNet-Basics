using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Configuration.Options.Controllers;

/// <summary>
/// Три інтерфейси доставки опцій. Усі три беруться з DI як параметри дії:
///   • <see cref="IOptions{T}"/>         — Singleton, обчислюється один раз;
///   • <see cref="IOptionsSnapshot{T}"/> — Scoped, перечитується раз на запит;
///   • <see cref="IOptionsMonitor{T}"/>  — Singleton із «живим» CurrentValue.
/// </summary>
[ApiController]
public sealed class OptionsController : ControllerBase
{
    // Повне дерево «ключ → значення (+ джерело)» — зручно для діагностики.
    [HttpGet("/sources")]
    public ContentResult Sources([FromServices] IConfiguration config)
        => Content(((IConfigurationRoot)config).GetDebugView());

    // IOptions<T> — Singleton. Обчислюється один раз. Підходить для більшості випадків.
    [HttpGet("/options/io")]
    public CatalogOptions Io([FromServices] IOptions<CatalogOptions> opt) => opt.Value;

    // IOptionsSnapshot<T> — Scoped. Перечитується раз на запит: зміни в appsettings.json
    // підхопляться на наступному запиті. У middleware / singleton не інжектиться.
    [HttpGet("/options/snapshot")]
    public CatalogOptions Snapshot([FromServices] IOptionsSnapshot<CatalogOptions> opt) => opt.Value;

    // IOptionsMonitor<T> — Singleton із CurrentValue та підпискою OnChange.
    // Єдиний варіант «живих» опцій для middleware і фонових сервісів.
    [HttpGet("/options/monitor")]
    public CatalogOptions Monitor([FromServices] IOptionsMonitor<CatalogOptions> mon) => mon.CurrentValue;
}

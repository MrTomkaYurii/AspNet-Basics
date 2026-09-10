using Microsoft.AspNetCore.Mvc;

namespace DependencyInjection.Lifetimes.Controllers;

/// <summary>
/// Контролер створюється заново на КОЖЕН запит, тож усе, що впроваджено в його
/// конструктор, живе в межах одного запиту:
///   • <see cref="IScopedOperation"/>  — той самий екземпляр, що бачить middleware;
///   • <see cref="ISingletonOperation"/> — один на весь застосунок;
///   • <see cref="ITransientOperation"/> — новий на кожен резолв (беремо в дії двічі).
/// </summary>
[ApiController]
public sealed class LifetimesController(
    IScopedOperation scoped,
    ISingletonOperation singleton) : ControllerBase
{
    // GET /ids
    [HttpGet("/ids")]
    public object GetIds(
        [FromServices] ITransientOperation transientA,
        [FromServices] ITransientOperation transientB)
        => new
        {
            transientA = transientA.Id,                        // ← різні між собою
            transientB = transientB.Id,
            scoped = scoped.Id,                                // ← дорівнює scopedFromMiddleware
            scopedFromMiddleware = HttpContext.Items["scopedFromMiddleware"],
            singleton = singleton.Id,                          // ← однаковий завжди
        };

    // GET /scopes — як користуватися Scoped із Singleton / фонового сервісу:
    // створити область вручну.
    [HttpGet("/scopes")]
    public object GetScopes([FromServices] IServiceScopeFactory scopeFactory)
    {
        using var a = scopeFactory.CreateScope();
        using var b = scopeFactory.CreateScope();
        return new
        {
            scopeA = a.ServiceProvider.GetRequiredService<IScopedOperation>().Id,
            scopeB = b.ServiceProvider.GetRequiredService<IScopedOperation>().Id,   // різні області → різні Id
        };
    }
}

using System.Diagnostics;

namespace Pipeline.CustomMiddleware.Middleware;

/// <summary>
/// Middleware «за конвенцією» — без інтерфейсу.
///
/// Правила:
///   • конструктор, перший параметр — <see cref="RequestDelegate"/> next;
///   • метод <c>InvokeAsync(HttpContext)</c> → Task;
///   • екземпляр створюється ОДИН раз на весь застосунок (тому в конструктор —
///     лише Singleton-залежності; Scoped — параметром InvokeAsync);
///   • реєструвати в DI не треба.
/// </summary>
public sealed class RequestTimingMiddleware(RequestDelegate next,
    ILogger<RequestTimingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        await next(context);
        logger.LogInformation("{Method} {Path} → {Ms} мс",
            context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds);
    }
}

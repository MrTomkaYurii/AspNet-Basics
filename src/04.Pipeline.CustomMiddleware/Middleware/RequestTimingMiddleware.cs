using System.Diagnostics;

namespace Pipeline.CustomMiddleware.Middleware;

/// <summary>
/// Middleware «за конвенцією» (convention-based).
///
/// Правила конвенції:
///   • публічний конструктор із першим параметром <see cref="RequestDelegate"/> (next);
///   • публічний метод <c>InvokeAsync(HttpContext)</c> (або <c>Invoke</c>), що повертає Task;
///   • додаткові залежності конструктора резолвляться з DI ОДИН раз на весь застосунок
///     (тому в конструктор можна брати лише Singleton-сервіси!);
///   • залежності на кожен запит — параметрами <c>InvokeAsync</c>.
///
/// Екземпляр створюється один раз і перевикористовується — сам клас має бути без стану.
/// </summary>
public sealed class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.GetTimestamp();

        // Response.OnStarting — надійний спосіб дописати заголовок:
        // колбек викликається безпосередньо перед відправкою заголовків клієнту,
        // коли ще нічого не «поїхало».
        context.Response.OnStarting(() =>
        {
            var elapsed = Stopwatch.GetElapsedTime(sw);
            context.Response.Headers["Server-Timing"] = $"app;dur={elapsed.TotalMilliseconds:F1}";
            return Task.CompletedTask;
        });

        await next(context);

        logger.LogInformation("{Method} {Path} → {Status} ({Elapsed:F1} мс)",
            context.Request.Method, context.Request.Path, context.Response.StatusCode,
            Stopwatch.GetElapsedTime(sw).TotalMilliseconds);
    }
}

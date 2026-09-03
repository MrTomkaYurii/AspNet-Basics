namespace Pipeline.CustomMiddleware.Middleware;

/// <summary>
/// Middleware на основі інтерфейсу <see cref="IMiddleware"/> (factory-based).
///
/// Відмінності від конвенції:
///   • екземпляр створюється фабрикою <c>IMiddlewareFactory</c> на КОЖЕН запит;
///   • отже, можна впроваджувати Scoped-залежності прямо в конструктор;
///   • обов'язкова реєстрація в DI: <c>services.AddScoped&lt;CorrelationIdMiddleware&gt;()</c>.
///
/// Компроміс: трохи більше церемоній, зате суворіша типізація й прозорий час життя.
/// </summary>
public sealed class CorrelationIdMiddleware(ILogger<CorrelationIdMiddleware> logger) : IMiddleware
{
    private const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Беремо id від клієнта (наскрізне трасування) або генеруємо новий.
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var incoming)
            ? incoming.ToString()
            : Guid.NewGuid().ToString();

        context.Response.Headers[HeaderName] = correlationId;

        // Кладемо у Features, щоб endpoint-и та інші middleware могли дістати.
        context.Items["CorrelationId"] = correlationId;

        // Логи в межах цього запиту матимуть властивість CorrelationId.
        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context);
        }
    }
}

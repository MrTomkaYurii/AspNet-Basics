namespace Pipeline.CustomMiddleware.Middleware;

/// <summary>
/// Middleware на основі інтерфейсу <see cref="IMiddleware"/>.
///
/// Відмінності від конвенції:
///   • новий екземпляр на КОЖЕН запит → у конструктор можна брати Scoped-залежності;
///   • сигнатура фіксована: <c>InvokeAsync(HttpContext, RequestDelegate)</c>;
///   • ОБОВ'ЯЗКОВА реєстрація в DI: <c>services.AddScoped&lt;CorrelationIdMiddleware&gt;()</c>.
/// </summary>
public sealed class CorrelationIdMiddleware : IMiddleware
{
    private const string Header = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Беремо id від клієнта або генеруємо новий.
        var id = context.Request.Headers[Header].FirstOrDefault() ?? Guid.NewGuid().ToString();

        context.Response.Headers[Header] = id;
        context.Items["CorrelationId"] = id;   // доступно endpoint-ам і решті middleware

        await next(context);
    }
}

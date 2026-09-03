namespace Pipeline.CustomMiddleware.Middleware;

/// <summary>
/// Методи-розширення <c>UseXxx()</c> — стандартний спосіб «сховати» реєстрацію
/// middleware за одним зрозумілим викликом (як <c>app.UseRouting()</c>,
/// <c>app.UseAuthentication()</c> у фреймворку).
/// </summary>
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder app)
        => app.UseMiddleware<RequestTimingMiddleware>();

    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}

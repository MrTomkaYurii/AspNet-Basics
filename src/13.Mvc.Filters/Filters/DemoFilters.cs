using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mvc.Filters.Filters;

// Кожен фільтр пише свій слід у список context.HttpContext.Items["trace"],
// щоб у відповіді було видно РЕАЛЬНИЙ порядок виконання.

file static class TraceExtensions
{
    public static void Trace(this FilterContext ctx, string step)
    {
        var list = (List<string>)(ctx.HttpContext.Items["trace"] ??= new List<string>());
        list.Add(step);
    }
    public static void Trace(this ExceptionContext ctx, string step)
    {
        var list = (List<string>)(ctx.HttpContext.Items["trace"] ??= new List<string>());
        list.Add(step);
    }
}

/// <summary>Resource filter — найзовнішній (після Authorization). Огортає навіть
/// model binding. Тут вимірюємо час і можемо зробити кешування/short-circuit.</summary>
public sealed class TimingResourceFilter : IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        context.HttpContext.Items["sw"] = System.Diagnostics.Stopwatch.StartNew();
        context.Trace("1: ResourceFilter.Executing");
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        var sw = (System.Diagnostics.Stopwatch)context.HttpContext.Items["sw"]!;
        sw.Stop();
        context.HttpContext.Response.Headers["X-Elapsed-Ms"] = sw.ElapsedMilliseconds.ToString();
        context.Trace("7: ResourceFilter.Executed");
    }
}

/// <summary>Action filter — навколо самого методу дії, вже зі зв'язаними аргументами.</summary>
public sealed class LoggingActionFilter(ILogger<LoggingActionFilter> logger) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        logger.LogInformation("Виклик {Action}, аргументів: {Count}",
            context.ActionDescriptor.DisplayName, context.ActionArguments.Count);
        context.Trace("3: ActionFilter.Executing");
    }

    public void OnActionExecuted(ActionExecutedContext context)
        => context.Trace("5: ActionFilter.Executed");
}

/// <summary>Result filter — навколо виконання результату (серіалізації відповіді).</summary>
public sealed class EnvelopeResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        context.Trace("6: ResultFilter.Executing");

        // Приклад втручання: огортаємо ObjectResult у «конверт».
        if (context.Result is ObjectResult { Value: not null } obj)
        {
            obj.Value = new
            {
                data = obj.Value,
                trace = context.HttpContext.Items["trace"],
            };
        }
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}

/// <summary>Exception filter — ловить необроблені винятки з дії та фільтрів дії.</summary>
public sealed class DemoExceptionFilter(IHostEnvironment env) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        context.Trace("X: ExceptionFilter");
        context.Result = new ObjectResult(new
        {
            error = "Оброблено ExceptionFilter-ом",
            type = context.Exception.GetType().Name,
            detail = env.IsDevelopment() ? context.Exception.Message : null,
        })
        { StatusCode = StatusCodes.Status500InternalServerError };
        context.ExceptionHandled = true;
    }
}

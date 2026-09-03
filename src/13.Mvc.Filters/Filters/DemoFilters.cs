using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mvc.Filters.Filters;

// Спільний «журнал» кроків цього запиту — щоб у відповіді було видно РЕАЛЬНИЙ
// порядок виконання фільтрів.
static class Steps
{
    public static List<string> Of(HttpContext http) =>
        (List<string>)(http.Items["steps"] ??= new List<string>());
}

/// <summary>Resource filter — після Authorization, ДО model binding. Кеш, short-circuit.</summary>
public sealed class DemoResourceFilter : IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext c) => Steps.Of(c.HttpContext).Add("resource: executing");
    public void OnResourceExecuted(ResourceExecutedContext c) => Steps.Of(c.HttpContext).Add("resource: executed");
}

/// <summary>Action filter — навколо методу дії, вже зі зв'язаними аргументами.</summary>
public sealed class DemoActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext c) => Steps.Of(c.HttpContext).Add("action: executing");
    public void OnActionExecuted(ActionExecutedContext c) => Steps.Of(c.HttpContext).Add("action: executed");
}

/// <summary>Result filter — навколо виконання IActionResult. Тут — огортаємо відповідь у «конверт».</summary>
public sealed class DemoResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext c)
    {
        Steps.Of(c.HttpContext).Add("result: executing");
        if (c.Result is ObjectResult obj)
            obj.Value = new { data = obj.Value, steps = Steps.Of(c.HttpContext) };
    }

    public void OnResultExecuted(ResultExecutedContext c) { }
}

/// <summary>Exception filter — ловить необроблені винятки з дії та action-фільтрів.</summary>
public sealed class DemoExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext c)
    {
        var steps = Steps.Of(c.HttpContext);
        steps.Add("exception filter");
        c.Result = new ObjectResult(new { error = c.Exception.Message, steps })
        {
            StatusCode = StatusCodes.Status500InternalServerError,
        };
        c.ExceptionHandled = true;
    }
}

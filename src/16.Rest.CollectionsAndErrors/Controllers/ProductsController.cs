using Common.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Rest.CollectionsAndErrors.Controllers;

/// <summary>
/// Колекція товарів: фільтр і сторінка приходять із query string. Шлях
/// <c>/products</c> — це ідентичність колекції, він не змінюється.
/// Помилки — у форматі <c>ProblemDetails</c> / <c>ValidationProblemDetails</c>.
/// </summary>
[ApiController]
[Route("products")]
public sealed class ProductsController(ICatalog catalog) : ControllerBase
{
    // GET /products?q=&page=1&pageSize=10
    [HttpGet]
    public ActionResult<Page<Product>> GetPage(
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageSize is < 1 or > 100)
        {
            ModelState.AddModelError(nameof(pageSize), "Має бути в діапазоні 1..100.");
            return ValidationProblem(ModelState);
        }

        var filtered = catalog.GetProducts()
            .Where(p => string.IsNullOrWhiteSpace(q)
                     || p.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var pageNumber = Math.Max(page, 1);
        var items = filtered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        Response.Headers["X-Total-Count"] = filtered.Count.ToString();
        return new Page<Product>(items, pageNumber, pageSize, filtered.Count);
    }

    // GET /products/{id} — помилка «немає ресурсу» у форматі ProblemDetails,
    // з додатковим полем productId.
    [HttpGet("{id:int}")]
    public ActionResult<Product> GetById(int id)
    {
        if (catalog.FindProduct(id) is { } p)
            return p;

        return Problem(
            title: "Товар не знайдено",
            statusCode: StatusCodes.Status404NotFound,
            extensions: new Dictionary<string, object?> { ["productId"] = id });
    }
}

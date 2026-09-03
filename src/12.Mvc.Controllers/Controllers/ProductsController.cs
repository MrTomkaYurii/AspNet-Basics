using Common.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Mvc.Controllers.Controllers;

/// <summary>
/// [ApiController] вмикає набір зручностей для API:
///   • автоматичну відповідь 400 з ValidationProblemDetails при невалідній ModelState;
///   • виведення джерела прив'язки (складний тип → тіло, простий → route/query);
///   • автоматичне обгортання проблемних статус-кодів у ProblemDetails;
///   • вимогу атрибутної маршрутизації (без [Route] — виняток на старті).
/// </summary>
[ApiController]
[Route("api/[controller]")]           // → /api/products  ([controller] = ім'я класу без суфікса)
[Produces("application/json")]
public sealed class ProductsController(ICatalog catalog) : ControllerBase
{
    // GET /api/products
    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetAll([FromQuery] string? search)
    {
        var items = catalog.GetProducts()
            .Where(p => string.IsNullOrWhiteSpace(search)
                        || p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        return Ok(items);
    }

    // GET /api/products/42
    // ActionResult<T> дозволяє повернути АБО модель (200), АБО інший результат (404).
    [HttpGet("{id:int}", Name = nameof(GetById))]
    [ProducesResponseType<Product>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Product> GetById(int id)
        => catalog.FindProduct(id) is { } p ? p : NotFound();

    // POST /api/products
    // Тіло прив'язується автоматично (складний тип). Якщо ModelState невалідна —
    // [ApiController] поверне 400 ще ДО входу в метод.
    [HttpPost]
    [ProducesResponseType<Product>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<Product> Create(ProductInput input)
    {
        // Крос-польова перевірка — вручну додаємо помилку в ModelState.
        if (catalog.FindCategory(input.CategoryId) is null)
        {
            ModelState.AddModelError(nameof(ProductInput.CategoryId), "Категорії з таким Id немає.");
            return ValidationProblem(ModelState);
        }

        var created = catalog.Add(input);

        // 201 + Location: /api/products/{id} через ім'я маршруту.
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/products/42
    [HttpPut("{id:int}")]
    public ActionResult<Product> Replace(int id, ProductInput input)
        => catalog.Replace(id, input) is { } updated ? updated : NotFound();

    // DELETE /api/products/42
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Delete(int id)
    {
        catalog.Delete(id);
        return NoContent();
    }
}

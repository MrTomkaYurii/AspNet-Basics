using Common.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Mvc.Controllers.Controllers;

/// <summary>
/// [ApiController] вмикає зручності для API:
///   • авто-400 з ValidationProblemDetails при невалідній ModelState (до входу в метод);
///   • виведення джерела прив'язки (складний тип → тіло, простий → route/query);
///   • ProblemDetails для проблемних статус-кодів;
///   • обов'язкову атрибутну маршрутизацію.
/// </summary>
[ApiController]
[Route("api/[controller]")]   // [controller] → "products" (ім'я класу без суфікса)
public sealed class ProductsController(ICatalog catalog) : ControllerBase
{
    // GET /api/products
    [HttpGet]
    public IEnumerable<Product> GetAll() => catalog.GetProducts();

    // GET /api/products/42
    // ActionResult<T>: повернути АБО модель (200), АБО інший результат (404).
    [HttpGet("{id:int}")]
    public ActionResult<Product> GetById(int id)
        => catalog.FindProduct(id) is { } p ? p : NotFound();

    // POST /api/products
    // ProductInput перевіряється автоматично ([ApiController] + DataAnnotations).
    [HttpPost]
    public ActionResult<Product> Create(ProductInput input)
    {
        // Крос-польове правило — вручну через ModelState.
        if (catalog.FindCategory(input.CategoryId) is null)
        {
            ModelState.AddModelError(nameof(input.CategoryId), "Категорії з таким Id немає.");
            return ValidationProblem(ModelState);
        }

        var created = catalog.Add(input);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);   // 201 + Location
    }

    // PUT /api/products/42
    [HttpPut("{id:int}")]
    public ActionResult<Product> Replace(int id, ProductInput input)
        => catalog.Replace(id, input) is { } updated ? updated : NotFound();

    // DELETE /api/products/42
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        catalog.Delete(id);
        return NoContent();
    }
}

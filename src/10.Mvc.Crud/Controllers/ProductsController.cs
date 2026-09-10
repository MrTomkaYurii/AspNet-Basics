using Common.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Mvc.Crud.Controllers;

/// <summary>
/// Один контролер = один ресурс. Атрибут <c>[Route("products")]</c> задає базовий
/// шлях; кожна дія додає метод і, за потреби, сегмент (<c>{id:int}</c>).
///
/// Коди відповідей CRUD:
///   LIST    GET    /products        → 200 + масив
///   READ    GET    /products/{id}   → 200 | 404
///   CREATE  POST   /products        → 201 + Location
///   REPLACE PUT    /products/{id}   → 200 | 404   (ідемпотентний)
///   DELETE  DELETE /products/{id}   → 204          (ідемпотентний)
/// </summary>
[ApiController]
[Route("products")]
public sealed class ProductsController(ICatalog catalog) : ControllerBase
{
    // LIST → 200 + масив (порожній масив — теж успіх).
    [HttpGet]
    public IEnumerable<Product> GetAll() => catalog.GetProducts();

    // READ → 200 або 404. Name — щоб CreatedAtAction побудував Location.
    [HttpGet("{id:int}", Name = "GetProductById")]
    public ActionResult<Product> GetById(int id)
        => catalog.FindProduct(id) is { } p ? p : NotFound();

    // CREATE → 201 + заголовок Location на новий ресурс.
    [HttpPost]
    public ActionResult<Product> Create(ProductInput input)
    {
        // Єдина перевірка — бізнес-правило (категорія існує?).
        if (catalog.FindCategory(input.CategoryId) is null)
            return BadRequest("Категорії з таким Id немає.");

        var created = catalog.Add(input);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // REPLACE (повна заміна) → 200 або 404. Ідемпотентний.
    [HttpPut("{id:int}")]
    public ActionResult<Product> Replace(int id, ProductInput input)
        => catalog.Replace(id, input) is { } updated ? updated : NotFound();

    // DELETE → 204. Ідемпотентний: 204 навіть якщо вже видалено.
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        catalog.Delete(id);
        return NoContent();
    }
}

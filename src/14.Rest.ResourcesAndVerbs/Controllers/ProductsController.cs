using Common.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Rest.ResourcesAndVerbs.Controllers;

/// <summary>
/// Ресурс «товар» і стандартні дієслова над ним. Дивіться коди відповідей і
/// властивості методів (safe / idempotent) — таблиця в NOTES.md.
/// </summary>
[ApiController]
[Route("products")]
public sealed class ProductsController(ICatalog catalog) : ControllerBase
{
    // GET колекція → завжди 200 (порожній масив — теж успіх).
    [HttpGet]
    public IEnumerable<Product> GetAll() => catalog.GetProducts();

    // GET елемент → 200 або 404.
    [HttpGet("{id:int}")]
    public ActionResult<Product> GetById(int id)
        => catalog.FindProduct(id) is { } p ? p : NotFound();

    // POST на колекцію → 201 Created + Location. НЕ ідемпотентний.
    // Невалідне тіло відсіює авто-400 від [ApiController] ще до цього методу.
    [HttpPost]
    public ActionResult<Product> Create(ProductInput input)
    {
        if (catalog.FindCategory(input.CategoryId) is null)
            return BadRequest("Категорії з таким Id немає.");

        var created = catalog.Add(input);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT на елемент → повна заміна. ІДЕМПОТЕНТНИЙ: той самий запит двічі → той самий стан.
    [HttpPut("{id:int}")]
    public ActionResult<Product> Replace(int id, ProductInput input)
        => catalog.Replace(id, input) is { } updated ? updated : NotFound();

    // DELETE → 204. ІДЕМПОТЕНТНИЙ: видалення вже видаленого — теж 204, а не помилка.
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        catalog.Delete(id);
        return NoContent();
    }

    // PATCH — часткова зміна. Деталі (JSON Patch / merge) — у прикладі 15.
    [HttpPatch("{id:int}")]
    public IActionResult Patch(int id) => StatusCode(StatusCodes.Status501NotImplemented);
}

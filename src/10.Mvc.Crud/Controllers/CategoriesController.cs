using Common.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Mvc.Crud.Controllers;

/// <summary>Довідник категорій — лише читання.</summary>
[ApiController]
[Route("categories")]
public sealed class CategoriesController(ICatalog catalog) : ControllerBase
{
    // GET /categories
    [HttpGet]
    public IEnumerable<Category> GetAll() => catalog.GetCategories();

    // GET /categories/1
    [HttpGet("{id:int}")]
    public ActionResult<Category> GetById(int id)
        => catalog.FindCategory(id) is { } c ? c : NotFound();
}

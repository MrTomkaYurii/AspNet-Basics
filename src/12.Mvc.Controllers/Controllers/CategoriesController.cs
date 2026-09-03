using Common.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Mvc.Controllers.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController(ICatalog catalog) : ControllerBase
{
    // GET /api/categories
    [HttpGet]
    public IEnumerable<Category> GetAll() => catalog.GetCategories();

    // GET /api/categories/1
    [HttpGet("{id:int}")]
    public ActionResult<Category> GetById(int id)
        => catalog.FindCategory(id) is { } c ? c : NotFound();
}

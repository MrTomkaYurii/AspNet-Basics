using Common.Domain;
using Microsoft.AspNetCore.Mvc;

namespace OpenApi.Swagger.Controllers;

[ApiController]
[Route("products")]
[Tags("Products")]
public sealed class ProductsController(ICatalog catalog) : ControllerBase
{
    /// <summary>Список товарів</summary>
    [HttpGet]
    public IEnumerable<Product> GetAll() => catalog.GetProducts();

    /// <summary>Отримати товар за Id</summary>
    /// <remarks>Коди <c>200</c> і <c>404</c> у документі — з атрибутів нижче.</remarks>
    [HttpGet("{id:int}")]
    [ProducesResponseType<Product>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Product> GetById(int id)
        => catalog.FindProduct(id) is { } p ? p : NotFound();

    /// <summary>Створити товар</summary>
    [HttpPost]
    [ProducesResponseType<Product>(StatusCodes.Status201Created)]
    public ActionResult<Product> Create(ProductInput input)
    {
        var created = catalog.Add(input);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}

[ApiController]
[Route("categories")]
[Tags("Categories")]
public sealed class CategoriesController(ICatalog catalog) : ControllerBase
{
    /// <summary>Список категорій</summary>
    [HttpGet]
    public IEnumerable<Category> GetAll() => catalog.GetCategories();
}

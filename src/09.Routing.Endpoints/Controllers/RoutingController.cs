using Common.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Routing.Endpoints.Controllers;

/// <summary>
/// Атрибутна маршрутизація: шаблон задають <c>[HttpGet("...")]</c> / <c>[Route("...")]</c>.
/// Провідний «/» у шаблоні робить його абсолютним (ігнорує префікс контролера).
/// Ті самі можливості, що й у Minimal API: constraints, значення за замовчуванням,
/// catch-all, іменовані маршрути, <see cref="LinkGenerator"/>.
/// </summary>
[ApiController]
public sealed class RoutingController(ICatalog catalog) : ControllerBase
{
    // Constraint :int — несумісний сегмент дає 404 (маршрут «не той»), а не 400.
    // Name — ім'я маршруту для генерації URL (див. GetLink нижче).
    [HttpGet("/products/{id:int}", Name = "product")]
    public ActionResult<Product> GetProduct(int id)
        => catalog.FindProduct(id) is { } p ? Ok(p) : NotFound();

    // Літеральний сегмент конкретніший за параметр → виграє в /products/{id}.
    [HttpGet("/products/featured")]
    public string Featured() => "Спеціальний маршрут (конкретніший за /products/{id}).";

    // Кілька параметрів + constraint :guid.
    [HttpGet("/products/{id:int}/reviews/{reviewId:guid}")]
    public object Review(int id, Guid reviewId) => new { productId = id, reviewId };

    // :alpha — лише літери (бізнес-ключ / slug).
    [HttpGet("/categories/{slug:alpha}")]
    public ActionResult<Category> Category(string slug)
        => catalog.GetCategories().FirstOrDefault(c => c.Slug == slug) is { } c ? Ok(c) : NotFound();

    // Значення за замовчуванням просто в шаблоні.
    [HttpGet("/pages/{page:int=1}")]
    public object Page(int page) => new { page };

    // Catch-all: {**path} захоплює решту шляху разом зі слешами.
    [HttpGet("/files/{**path}")]
    public object Files(string path) => new { path };

    // Генерація URL: не склеюйте рядки — просіть у маршрутизатора за ІМЕНЕМ маршруту.
    [HttpGet("/link/{id:int}")]
    public object GetLink(int id, [FromServices] LinkGenerator links)
        => new { url = links.GetUriByName(HttpContext, "product", new { id }) };
}

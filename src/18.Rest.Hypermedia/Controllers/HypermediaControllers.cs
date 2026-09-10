using Common.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Rest.Hypermedia.Controllers;

/// <summary>Точка входу: далі рухаємось лише за <c>_links</c>.</summary>
[ApiController]
public sealed class RootController : ControllerBase
{
    [HttpGet("/")]
    public object Index() => new { _links = new { products = "/products", cart = "/cart" } };
}

[ApiController]
[Route("products")]
public sealed class ProductsController(ICatalog catalog) : ControllerBase
{
    // Колекція: у кожного елемента є self-посилання.
    [HttpGet]
    public object GetAll() => new
    {
        items = catalog.GetProducts().Select(p => new
        {
            p.Id, p.Name, p.Price,
            _links = new { self = $"/products/{p.Id}" },
        }),
    };

    // Елемент: посилання на пов'язані ресурси й можливі дії.
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
        => catalog.FindProduct(id) is { } p
            ? Ok(new
            {
                p.Id, p.Name, p.Price,
                _links = new
                {
                    self = $"/products/{id}",
                    addToCart = new { href = "/cart/items", method = "POST" },
                },
            })
            : NotFound();
}

/// <summary>Кошик: доступні дії ЗАЛЕЖАТЬ ВІД СТАНУ (див. <see cref="Cart.View"/>).</summary>
[ApiController]
[Route("cart")]
public sealed class CartController(Cart cart, ICatalog catalog) : ControllerBase
{
    [HttpGet]
    public object Get() => cart.View();

    [HttpPost("items")]
    public IActionResult AddItem(AddItem body)
    {
        if (catalog.FindProduct(body.ProductId) is null)
            return BadRequest("Такого товару немає.");

        cart.Add(body.ProductId);
        return Ok(cart.View());
    }

    [HttpPost("checkout")]
    public IActionResult Checkout()
    {
        if (cart.IsEmpty)
            return Conflict("Кошик порожній — оформлення недоступне.");

        cart.Clear();
        return Ok(new { status = "confirmed" });
    }
}

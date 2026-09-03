using Common;
using Common.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 18. REST: гіпермедіа (HATEOAS)
//
// Ідея: клієнт не «зашиває» URL-и, а йде за посиланнями (`_links`), які сервер
// повертає у відповіді. Набір посилань залежить від СТАНУ ресурсу — сервер
// підказує, що можна зробити далі. Це рівень 3 моделі зрілості Річардсона.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ======================================================================
//  1 · СЕРВІСИ
// ======================================================================
builder.Services.AddCatalog();
builder.Services.AddSingleton<Cart>();
builder.Services.AddApiDocs();

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР
// ======================================================================
app.MapApiDocs();

// ======================================================================
//  3 · ЗАПИТИ
// ======================================================================

// Точка входу: далі рухаємось лише за _links.
app.MapGet("/", () => new { _links = new { products = "/products", cart = "/cart" } });

// Колекція: у кожного елемента є self-посилання.
app.MapGet("/products", (ICatalog catalog) => new
{
    items = catalog.GetProducts().Select(p => new
    {
        p.Id, p.Name, p.Price,
        _links = new { self = $"/products/{p.Id}" },
    }),
});

// Елемент: посилання на пов'язані ресурси й можливі дії.
app.MapGet("/products/{id:int}", (int id, ICatalog catalog) =>
    catalog.FindProduct(id) is { } p
        ? Results.Ok(new
        {
            p.Id, p.Name, p.Price,
            _links = new
            {
                self = $"/products/{id}",
                addToCart = new { href = "/cart/items", method = "POST" },
            },
        })
        : Results.NotFound());

// ── Кошик: доступні дії ЗАЛЕЖАТЬ ВІД СТАНУ ────────────────────────────────
app.MapGet("/cart", (Cart cart) => cart.View());

app.MapPost("/cart/items", (AddItem body, Cart cart, ICatalog catalog) =>
{
    if (catalog.FindProduct(body.ProductId) is null)
        return Results.BadRequest("Такого товару немає.");

    cart.Add(body.ProductId);
    return Results.Ok(cart.View());
});

app.MapPost("/cart/checkout", (Cart cart) =>
{
    if (cart.IsEmpty)
        return Results.Conflict("Кошик порожній — оформлення недоступне.");

    cart.Clear();
    return Results.Ok(new { status = "confirmed" });
});

app.Run();

record AddItem(int ProductId);

/// <summary>Демо-кошик (один на застосунок). Ключове — метод View: різні _links
/// залежно від того, порожній кошик чи ні.</summary>
public sealed class Cart
{
    private readonly List<int> _productIds = [];

    public bool IsEmpty => _productIds.Count == 0;
    public void Add(int productId) => _productIds.Add(productId);
    public void Clear() => _productIds.Clear();

    public object View()
    {
        var links = new Dictionary<string, object>
        {
            ["addItem"] = new { href = "/cart/items", method = "POST" },
        };
        if (!IsEmpty)
            links["checkout"] = new { href = "/cart/checkout", method = "POST" };

        return new { productIds = _productIds, _links = links };
    }
}

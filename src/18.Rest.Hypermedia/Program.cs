using Common;
using Common.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// ПРИКЛАД 18. REST: гіпермедіа (HATEOAS)
//
// HATEOAS = Hypermedia As The Engine Of Application State.
// Ідея: клієнт не «зашиває» URL-и, а йде за посиланнями, які сервер віддає у
// відповіді. Набір доступних посилань залежить від СТАНУ ресурсу — сервер
// підказує, що можна зробити далі.
//
// Це рівень 3 «моделі зрілості Річардсона» (див. NOTES).
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCatalog();
builder.Services.AddSingleton<Cart>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<LinkBuilder>();
var app = builder.Build();

// Точка входу: клієнт починає звідси й далі рухається за _links.
app.MapGet("/", (LinkBuilder link) => Results.Ok(new
{
    service = "Catalog API",
    _links = new
    {
        self = link.To("/"),
        products = link.To("/products"),
        cart = link.To("/cart"),
    },
}));

// Колекція: посилання на пагінацію + self для кожного елемента.
app.MapGet("/products", (ICatalog catalog, LinkBuilder link) => Results.Ok(new
{
    items = catalog.GetProducts().Select(p => new
    {
        p.Id, p.Name, p.Price,
        _links = new { self = link.To($"/products/{p.Id}") },
    }),
    _links = new { self = link.To("/products") },
}));

// Елемент: посилання на пов'язані ресурси та можливі дії.
app.MapGet("/products/{id:int}", (int id, ICatalog catalog, LinkBuilder link) =>
{
    if (catalog.FindProduct(id) is not { } p) return Results.NotFound();
    return Results.Ok(new
    {
        p.Id, p.Name, p.Description, p.Price, p.Sku,
        _links = new
        {
            self = link.To($"/products/{p.Id}"),
            category = link.To($"/categories/{p.CategoryId}"),
            addToCart = link.Action($"/cart/items", "POST"),
        },
    });
});

app.MapGet("/categories/{id:int}", (int id, ICatalog catalog, LinkBuilder link) =>
    catalog.FindCategory(id) is { } c
        ? Results.Ok(new { c.Id, c.Name, c.Slug, _links = new { self = link.To($"/categories/{c.Id}") } })
        : Results.NotFound());

// ── Кошик: доступні дії ЗАЛЕЖАТЬ ВІД СТАНУ ────────────────────────────────
app.MapGet("/cart", (Cart cart, ICatalog catalog, LinkBuilder link) => Results.Ok(cart.Represent(catalog, link)));

app.MapPost("/cart/items", (AddItem body, Cart cart, ICatalog catalog, LinkBuilder link) =>
{
    if (catalog.FindProduct(body.ProductId) is null)
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["productId"] = ["Такого товару немає."],
        });

    cart.Add(body.ProductId, body.Quantity <= 0 ? 1 : body.Quantity);
    return Results.Ok(cart.Represent(catalog, link));
});

app.MapPost("/cart/checkout", (Cart cart, ICatalog catalog, LinkBuilder link) =>
{
    if (cart.IsEmpty)
        return Results.Problem("Кошик порожній — оформлення недоступне.", statusCode: StatusCodes.Status409Conflict);

    var orderId = cart.Checkout();
    return Results.Ok(new
    {
        orderId,
        status = "confirmed",
        _links = new { self = link.To($"/orders/{orderId}"), cart = link.To("/cart") },
    });
});

app.Run();

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Будує абсолютні URL відносно поточного запиту.</summary>
public sealed class LinkBuilder(IHttpContextAccessor accessor)
{
    private string Base
    {
        get
        {
            var r = accessor.HttpContext!.Request;
            return $"{r.Scheme}://{r.Host}";
        }
    }

    public object To(string path) => new { href = Base + path };
    public object Action(string path, string method) => new { href = Base + path, method };
}

public sealed record AddItem(int ProductId, int Quantity);

/// <summary>Демо-кошик (один на застосунок). Ключове — метод Represent,
/// що віддає РІЗНІ _links залежно від того, порожній кошик чи ні.</summary>
public sealed class Cart
{
    private readonly Dictionary<int, int> _items = new();
    private int _lastOrderId;

    public bool IsEmpty => _items.Count == 0;
    public void Add(int productId, int qty) => _items[productId] = _items.GetValueOrDefault(productId) + qty;
    public int Checkout() { _items.Clear(); return ++_lastOrderId + 1000; }

    public object Represent(ICatalog catalog, LinkBuilder link)
    {
        var lines = _items.Select(kv => new
        {
            productId = kv.Key,
            name = catalog.FindProduct(kv.Key)?.Name,
            quantity = kv.Value,
            _links = new { product = link.To($"/products/{kv.Key}") },
        }).ToArray();

        var total = _items.Sum(kv => (catalog.FindProduct(kv.Key)?.Price ?? 0) * kv.Value);

        // Стан → набір дій. Порожній кошик не пропонує checkout.
        var links = new Dictionary<string, object>
        {
            ["self"] = link.To("/cart"),
            ["addItem"] = link.Action("/cart/items", "POST"),
        };
        if (!IsEmpty)
            links["checkout"] = link.Action("/cart/checkout", "POST");

        return new { lines, total, _links = links };
    }
}

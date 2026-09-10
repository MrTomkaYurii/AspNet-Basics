namespace Rest.Hypermedia;

/// <summary>Тіло запиту «додати товар у кошик».</summary>
public sealed record AddItem(int ProductId);

/// <summary>
/// Демо-кошик (один на застосунок). Ключове — метод <see cref="View"/>: різні
/// <c>_links</c> залежно від того, порожній кошик чи ні.
/// </summary>
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

using System.Collections.Concurrent;
using Common.Domain;

namespace Common;

/// <summary>
/// Потокобезпечне сховище каталогу в пам'яті. Реєструється як <b>Singleton</b>
/// (див. <see cref="CatalogServiceCollectionExtensions"/>), тому стан живе
/// стільки ж, скільки застосунок, і спільний для всіх запитів.
///
/// Це навчальна реалізація: гонки типу «read-modify-write» між HTTP-запитами
/// тут можливі — приклад 15 показує, як їх закриває оптимістичне блокування (ETag).
/// </summary>
public sealed class InMemoryCatalog : ICatalog
{
    private readonly ConcurrentDictionary<int, Product> _products = new();
    private readonly List<Category> _categories;
    private int _nextId;

    public InMemoryCatalog()
    {
        _categories =
        [
            new Category(1, "Ноутбуки", "laptops"),
            new Category(2, "Периферія", "peripherals"),
            new Category(3, "Аксесуари", "accessories"),
        ];

        var now = DateTimeOffset.UtcNow;
        Product[] seed =
        [
            new(1, "ThinkPad X1 Carbon", "14\" ультрабук", 1899.00m, 1, "LAP-X1C-14", 1, now, now),
            new(2, "Mechanical Keyboard K3", "Компактна механіка", 89.90m, 2, "KEY-K3-75", 1, now, now),
            new(3, "USB-C Hub 7-in-1", null, 39.50m, 3, "ACC-HUB-7", 1, now, now),
        ];
        foreach (var p in seed)
            _products[p.Id] = p;

        _nextId = seed.Length;
    }

    public IReadOnlyList<Category> GetCategories() => _categories;

    public Category? FindCategory(int id) => _categories.FirstOrDefault(c => c.Id == id);

    public IReadOnlyList<Product> GetProducts() =>
        _products.Values.OrderBy(p => p.Id).ToArray();

    public Product? FindProduct(int id) =>
        _products.TryGetValue(id, out var p) ? p : null;

    public Product Add(ProductInput input)
    {
        var id = Interlocked.Increment(ref _nextId);
        var now = DateTimeOffset.UtcNow;
        var product = new Product(
            id, input.Name, input.Description, input.Price,
            input.CategoryId, input.Sku, Version: 1, CreatedAt: now, UpdatedAt: now);

        _products[id] = product;
        return product;
    }

    public Product? Replace(int id, ProductInput input)
    {
        if (!_products.TryGetValue(id, out var existing))
            return null;

        var updated = existing with
        {
            Name = input.Name,
            Description = input.Description,
            Price = input.Price,
            CategoryId = input.CategoryId,
            Sku = input.Sku,
            Version = existing.Version + 1,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        _products[id] = updated;
        return updated;
    }

    public bool Delete(int id) => _products.TryRemove(id, out _);
}

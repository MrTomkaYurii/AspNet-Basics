using Common.Domain;

namespace Common;

/// <summary>
/// Сховище каталогу в пам'яті. Реєструється як Singleton, тож стан спільний для
/// всіх запитів і живе стільки ж, скільки застосунок. Навчальна реалізація —
/// без блокувань і без бази даних.
/// </summary>
public sealed class InMemoryCatalog : ICatalog
{
    private readonly List<Category> _categories =
    [
        new(1, "Ноутбуки", "laptops"),
        new(2, "Периферія", "peripherals"),
        new(3, "Аксесуари", "accessories"),
    ];

    private readonly List<Product> _products =
    [
        new(1, "ThinkPad X1 Carbon", 1899m, 1),
        new(2, "Клавіатура K3", 89m, 2),
        new(3, "USB-C хаб", 39m, 3),
    ];

    private int _nextId = 3;

    public IReadOnlyList<Category> GetCategories() => _categories;
    public Category? FindCategory(int id) => _categories.FirstOrDefault(c => c.Id == id);

    public IReadOnlyList<Product> GetProducts() => _products;
    public Product? FindProduct(int id) => _products.FirstOrDefault(p => p.Id == id);

    public Product Add(ProductInput input)
    {
        var product = new Product(++_nextId, input.Name, input.Price, input.CategoryId);
        _products.Add(product);
        return product;
    }

    public Product? Replace(int id, ProductInput input)
    {
        var index = _products.FindIndex(p => p.Id == id);
        if (index < 0)
            return null;

        var updated = _products[index] with
        {
            Name = input.Name,
            Price = input.Price,
            CategoryId = input.CategoryId,
            Version = _products[index].Version + 1,
        };
        _products[index] = updated;
        return updated;
    }

    public bool Delete(int id) => _products.RemoveAll(p => p.Id == id) > 0;
}

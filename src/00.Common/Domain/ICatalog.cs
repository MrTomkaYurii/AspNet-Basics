namespace Common.Domain;

/// <summary>
/// Абстракція сховища каталогу. Приклади залежать від цього інтерфейсу, а не від
/// реалізації (<see cref="InMemoryCatalog"/>) — це Dependency Inversion.
/// </summary>
public interface ICatalog
{
    IReadOnlyList<Category> GetCategories();
    Category? FindCategory(int id);

    IReadOnlyList<Product> GetProducts();
    Product? FindProduct(int id);

    /// <summary>Створює товар (сервер призначає Id і Version = 1).</summary>
    Product Add(ProductInput input);

    /// <summary>Повна заміна (HTTP PUT). <c>null</c>, якщо товару немає. Version += 1.</summary>
    Product? Replace(int id, ProductInput input);

    /// <summary><c>true</c>, якщо товар було видалено.</summary>
    bool Delete(int id);
}

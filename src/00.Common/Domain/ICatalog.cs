namespace Common.Domain;

/// <summary>
/// Абстракція сховища каталогу. У реальному застосунку за нею була б база даних;
/// у курсі — реалізація в пам'яті (<see cref="InMemoryCatalog"/>).
///
/// Приклади залежать саме від інтерфейсу, а не від реалізації — це і є
/// Dependency Inversion: код прикладів не знає, звідки беруться дані.
/// </summary>
public interface ICatalog
{
    // --- Категорії (тільки читання) ---
    IReadOnlyList<Category> GetCategories();
    Category? FindCategory(int id);

    // --- Товари (повний CRUD) ---
    IReadOnlyList<Product> GetProducts();
    Product? FindProduct(int id);

    /// <summary>Створює товар. Сервер призначає Id, Version = 1 і дати.</summary>
    Product Add(ProductInput input);

    /// <summary>
    /// Повна заміна товару (семантика HTTP <c>PUT</c>). Повертає <c>null</c>,
    /// якщо товару з таким <paramref name="id"/> немає. Version збільшується на 1.
    /// </summary>
    Product? Replace(int id, ProductInput input);

    /// <summary>Видаляє товар. <c>true</c>, якщо щось було видалено.</summary>
    bool Delete(int id);
}

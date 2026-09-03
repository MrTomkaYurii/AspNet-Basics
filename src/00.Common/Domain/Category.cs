namespace Common.Domain;

/// <summary>
/// Категорія товарів каталогу. Незмінний (immutable) record — доменні дані
/// зручно передавати по конвеєру як value-об'єкт.
/// </summary>
/// <param name="Id">Стабільний ідентифікатор ресурсу.</param>
/// <param name="Name">Людиночитна назва, напр. "Ноутбуки".</param>
/// <param name="Slug">URL-дружній ключ, напр. "laptops".</param>
public sealed record Category(int Id, string Name, string Slug);

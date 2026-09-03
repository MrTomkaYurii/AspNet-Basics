using Common.Domain;

namespace Rest.Representations;

/// <summary>
/// <b>Представлення</b> ресурсу — окремий клас від доменної сутності
/// <see cref="Product"/>. Навмисно з відкритими сеттерами й конструктором без
/// параметрів: цього вимагають <c>XmlSerializer</c> і документ JSON Patch.
///
/// Це і є суть REST: клієнт обмінюється з сервером ПРЕДСТАВЛЕННЯМИ, а не
/// внутрішніми об'єктами. Формат (JSON/XML) — лише кодування представлення.
/// </summary>
public sealed class ProductRepresentation
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string Sku { get; set; } = "";
    public int Version { get; set; }

    public static ProductRepresentation From(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        CategoryId = p.CategoryId,
        Sku = p.Sku,
        Version = p.Version,
    };

    public ProductInput ToInput() => new()
    {
        Name = Name,
        Description = Description,
        Price = Price,
        CategoryId = CategoryId,
        Sku = Sku,
    };
}

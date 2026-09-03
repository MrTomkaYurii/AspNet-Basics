using Common.Domain;

namespace Rest.Representations;

/// <summary>
/// <b>Представлення</b> ресурсу — окремий клас від доменного <see cref="Product"/>.
/// Навмисно з відкритими сеттерами й конструктором без параметрів: цього
/// вимагають <c>XmlSerializer</c> і документ JSON Patch.
///
/// Суть REST: клієнт обмінюється з сервером ПРЕДСТАВЛЕННЯМИ, а не внутрішніми
/// об'єктами. JSON / XML — лише кодування того самого представлення.
/// </summary>
public sealed class ProductRepresentation
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public int Version { get; set; }

    public static ProductRepresentation From(Product p) => new()
    {
        Id = p.Id, Name = p.Name, Price = p.Price, CategoryId = p.CategoryId, Version = p.Version,
    };

    public ProductInput ToInput() => new() { Name = Name, Price = Price, CategoryId = CategoryId };
}

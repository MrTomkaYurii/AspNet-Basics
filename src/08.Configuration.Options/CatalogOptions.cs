using System.ComponentModel.DataAnnotations;

namespace Configuration.Options;

/// <summary>
/// Типізовані налаштування секції "Catalog". Патерн Options: один раз
/// прив'язуємо секцію конфігурації до класу — і далі впроваджуємо
/// <c>IOptions&lt;CatalogOptions&gt;</c> замість рядкових ключів по всьому коду.
/// </summary>
public sealed class CatalogOptions
{
    public const string SectionName = "Catalog";

    [Required, MinLength(3)]
    public string StoreName { get; init; } = "";

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;

    public bool ExperimentalSearch { get; init; }
}

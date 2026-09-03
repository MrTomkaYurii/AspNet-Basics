using System.ComponentModel.DataAnnotations;

namespace Configuration.Options;

/// <summary>
/// Строго типізовані налаштування розділу "Catalog" з конфігурації.
///
/// Патерн Options: замість того, щоб тягати <see cref="IConfiguration"/> й
/// читати рядкові ключі по всьому коду, ми один раз прив'язуємо секцію до класу
/// і впроваджуємо <c>IOptions&lt;CatalogOptions&gt;</c>.
/// </summary>
public sealed class CatalogOptions
{
    public const string SectionName = "Catalog";

    /// <summary>Назва вітрини, що показується клієнту.</summary>
    [Required, MinLength(3)]
    public string StoreName { get; init; } = "";

    /// <summary>Розмір сторінки за замовчуванням для списків.</summary>
    [Range(1, 100)]
    public int DefaultPageSize { get; init; } = 20;

    /// <summary>Валюта у форматі ISO 4217.</summary>
    [RegularExpression("^[A-Z]{3}$")]
    public string Currency { get; init; } = "USD";

    /// <summary>Чи показувати експериментальний пошук.</summary>
    public bool EnableExperimentalSearch { get; init; }
}

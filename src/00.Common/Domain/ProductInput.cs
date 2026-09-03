using System.ComponentModel.DataAnnotations;

namespace Common.Domain;

/// <summary>
/// Модель <b>запиту</b> (write model / DTO) для створення та оновлення товару.
/// Свідомо відокремлена від <see cref="Product"/>: клієнт не має права задавати
/// <c>Id</c>, <c>Version</c>, дати — це відповідальність сервера.
///
/// Атрибути валідації (<see cref="DataAnnotations"/>) живуть тут, щоб приклади
/// 11–16 могли перевикористовувати ті самі правила.
/// </summary>
public sealed class ProductInput
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; init; }

    [Range(0.01, 1_000_000)]
    public decimal Price { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "CategoryId має посилатися на наявну категорію.")]
    public int CategoryId { get; init; }

    [Required]
    [RegularExpression("^[A-Z0-9-]{3,32}$", ErrorMessage = "SKU: великі латинські літери, цифри та дефіс, 3–32 символи.")]
    public string Sku { get; init; } = string.Empty;
}

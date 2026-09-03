using System.ComponentModel.DataAnnotations;

namespace Common.Domain;

/// <summary>
/// Модель <b>запиту</b> для створення / оновлення товару. Окрема від
/// <see cref="Product"/>: клієнт не задає <c>Id</c> чи <c>Version</c> — це робота сервера.
///
/// Атрибути валідації тут — щоб приклади 11–12 і 14–16 могли їх перевикористати.
/// </summary>
public sealed class ProductInput
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = "";

    [Range(0.01, 100_000)]
    public decimal Price { get; init; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }
}

namespace Common.Domain;

/// <summary>
/// Товар каталогу — це <b>ресурс</b> у термінах REST: має стабільний URI виду
/// <c>/products/{id}</c> і кілька представлень (JSON, XML тощо).
/// </summary>
/// <param name="Id">Ідентифікатор ресурсу (генерує сервер).</param>
/// <param name="Name">Назва товару.</param>
/// <param name="Description">Опис; може бути відсутнім.</param>
/// <param name="Price">Ціна в умовних одиницях.</param>
/// <param name="CategoryId">Посилання на <see cref="Category"/>.</param>
/// <param name="Sku">Складський артикул (unique business key).</param>
/// <param name="Version">
/// Лічильник змін. Зростає на 1 при кожному оновленні й використовується
/// в прикладі 15 як значення для заголовка <c>ETag</c> / оптимістичного блокування.
/// </param>
/// <param name="CreatedAt">Момент створення (UTC).</param>
/// <param name="UpdatedAt">Момент останньої зміни (UTC).</param>
public sealed record Product(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int CategoryId,
    string Sku,
    int Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

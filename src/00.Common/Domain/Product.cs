namespace Common.Domain;

/// <summary>
/// Товар каталогу — це <b>ресурс</b> у термінах REST: має стабільний URI виду
/// <c>/products/{id}</c>.
/// </summary>
/// <param name="Id">Ідентифікатор ресурсу (генерує сервер).</param>
/// <param name="Name">Назва товару.</param>
/// <param name="Price">Ціна.</param>
/// <param name="CategoryId">Посилання на <see cref="Category"/>.</param>
/// <param name="Version">
/// Лічильник змін: +1 при кожному оновленні. Використовується лише в прикладі 15
/// (ETag / оптимістичне блокування). У решті прикладів можна не зважати.
/// </param>
public sealed record Product(
    int Id,
    string Name,
    decimal Price,
    int CategoryId,
    int Version = 1);

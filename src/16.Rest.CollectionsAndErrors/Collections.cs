namespace Rest.CollectionsAndErrors;

/// <summary>Параметри запиту колекції — приходять із query string.</summary>
public readonly record struct ProductQuery(string? Q, int Page = 1, int PageSize = 10);

/// <summary>Конверт відповіді: сторінка даних + метадані пагінації.</summary>
public sealed record Page<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int Total)
{
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}

namespace Rest.CollectionsAndErrors;

/// <summary>Параметри запиту колекції — фільтр, сортування, пагінація.
/// Прив'язуються з query через [AsParameters].</summary>
public readonly record struct ProductQuery(
    string? Q,
    string? Category,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Sort,          // напр. "price", "-price", "name"
    int? Page,
    int? PageSize)
{
    public int SafePage => Page is > 0 ? Page.Value : 1;
    public int SafePageSize => PageSize is > 0 and <= MaxPageSize ? PageSize.Value : DefaultPageSize;

    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;
    public static readonly string[] AllowedSortFields = ["id", "name", "price", "version"];
}

/// <summary>Конверт відповіді для колекції: дані + метадані пагінації.
/// Метадані можна дублювати в заголовках (X-Total-Count, Link) — так роблять
/// GitHub API та інші.</summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int Total)
{
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrev => Page > 1;
}

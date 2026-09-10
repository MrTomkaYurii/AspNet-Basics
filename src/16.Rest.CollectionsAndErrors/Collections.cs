namespace Rest.CollectionsAndErrors;

/// <summary>Конверт відповіді: сторінка даних + метадані пагінації.</summary>
public sealed record Page<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int Total)
{
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}

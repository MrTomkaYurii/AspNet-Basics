using System.Diagnostics.CodeAnalysis;

namespace MinimalApi.BindingAndValidation;

/// <summary>
/// Тип із власним розбором рядка. Якщо тип має статичний
/// <c>bool TryParse(string?, IFormatProvider?, out T)</c> (інтерфейс
/// <see cref="IParsable{TSelf}"/>), Minimal API вміє прив'язувати його
/// з route або query автоматично.
/// </summary>
public readonly record struct GeoPoint(double Lat, double Lon) : IParsable<GeoPoint>
{
    public static GeoPoint Parse(string s, IFormatProvider? provider)
        => TryParse(s, provider, out var result) ? result : throw new FormatException("Очікується 'lat,lon'.");

    public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out GeoPoint result)
    {
        result = default;
        var parts = s?.Split(',');
        if (parts is not { Length: 2 }) return false;
        if (!double.TryParse(parts[0], out var lat) || !double.TryParse(parts[1], out var lon)) return false;
        result = new GeoPoint(lat, lon);
        return true;
    }
}

/// <summary>
/// Модель, яку Minimal API «розкладає» на окремі параметри завдяки
/// <c>[AsParameters]</c>. Кожна властивість береться зі свого джерела:
/// маршрут, query, заголовок, сервіси.
/// </summary>
public readonly record struct ListProductsQuery(
    string? Search,
    int Page,
    int PageSize,
    string[]? Tags);

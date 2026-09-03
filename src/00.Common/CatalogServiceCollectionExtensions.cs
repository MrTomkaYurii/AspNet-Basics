using Common.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Common;

/// <summary>
/// Патерн "Add{Feature}" — стандарт ASP.NET Core: бібліотека дає один метод, що
/// інкапсулює свої реєстрації в DI. Порівняйте: <c>AddControllers()</c>, <c>AddAuthentication()</c>.
/// </summary>
public static class CatalogServiceCollectionExtensions
{
    public static IServiceCollection AddCatalog(this IServiceCollection services)
    {
        // Одне сховище в пам'яті на весь застосунок.
        services.AddSingleton<ICatalog, InMemoryCatalog>();
        return services;
    }
}

using Common.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Common;

/// <summary>
/// Метод-розширення для реєстрації каталогу в DI-контейнері.
///
/// Патерн "Add{Feature}" — стандарт ASP.NET Core: кожна бібліотека дає один
/// зрозумілий метод, який інкапсулює всі свої реєстрації. Порівняйте з
/// <c>builder.Services.AddControllers()</c>, <c>AddAuthentication()</c> тощо.
/// </summary>
public static class CatalogServiceCollectionExtensions
{
    public static IServiceCollection AddCatalog(this IServiceCollection services)
    {
        // Singleton: одне сховище в пам'яті на весь застосунок.
        // TryAdd — не перезатирає реєстрацію, якщо приклад підмінив ICatalog своєю.
        services.TryAddSingleton<ICatalog, InMemoryCatalog>();
        return services;
    }
}

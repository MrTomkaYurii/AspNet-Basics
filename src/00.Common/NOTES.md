# 00.Common — спільна доменна основа

Це **не приклад**, а бібліотека, яку підключають усі інші проекти курсу.
Тримає предметну область в одному місці, щоб приклади відрізнялися лише
тим, що вони демонструють у ASP.NET Core, а не бізнес-логікою.

## Що всередині

| Тип | Роль |
|---|---|
| `Domain/Category`, `Domain/Product` | Доменні сутності (immutable `record`). `Product` — це **ресурс** у REST. |
| `Domain/ProductInput` | Модель запиту (write model). Відокремлена від сутності: клієнт не задає `Id`, `Version`, дати. Тут же — атрибути валідації `DataAnnotations`. |
| `Domain/ICatalog` | Абстракція сховища. Приклади залежать від інтерфейсу — це **Dependency Inversion**. |
| `InMemoryCatalog` | Потокобезпечна реалізація в пам'яті з початковими даними. |
| `CatalogServiceCollectionExtensions.AddCatalog()` | Реєстрація в DI за патерном `Add{Feature}`. |

## Ключові концепції

- **DTO vs Entity.** Окрема модель для вводу захищає інваріанти: сервер сам
  керує ідентифікатором, версією та часовими мітками.
- **Патерн `Add{Feature}` + `IServiceCollection`.** Кожна бібліотека надає один
  метод розширення, що інкапсулює свої реєстрації. Так побудований увесь ASP.NET Core.
- **`record` та `with`.** Оновлення — це створення нового екземпляра
  (`existing with { Version = existing.Version + 1 }`), стан не мутується на місці.
- **Реєстрація Singleton.** Сховище в пам'яті живе стільки ж, скільки процес.
  Порівняйте з прикладом [07](../07.DependencyInjection.Lifetimes) про час життя сервісів.

## Посилання

- Dependency injection в ASP.NET Core: <https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection>
- Патерн Options та методи розширення `IServiceCollection`: <https://learn.microsoft.com/dotnet/core/extensions/dependency-injection>
- `record` types у C#: <https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record>

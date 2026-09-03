# 20. Інтеграційні тести HTTP API

## Що демонструємо

Тестування реального конвеєра застосунку через `WebApplicationFactory<T>` —
без запущеного сервера й мережі. Перевіряємо поведінку endpoint-ів прикладу 14
«очима клієнта»: коди відповідей, заголовки, ідемпотентність.

## Ключові концепції

### `WebApplicationFactory<TEntryPoint>`
- Піднімає застосунок **у пам'яті**: увесь DI, усі middleware, маршрутизація,
  серіалізація — справжні.
- `CreateClient()` дає `HttpClient`, що ходить у застосунок через `TestServer`
  (транспорт у пам'яті, не TCP).
- `TEntryPoint` — клас `Program` цільового проекту. Тому в прикладі 14 додано
  `public partial class Program;` (згенерований клас `Program` — `internal`).

### Піраміда тестів

| Рівень | Швидкість | Що ловить | Тут |
|---|---|---|---|
| Unit | мс | логіку методу | — |
| Integration (in-memory) | десятки мс | конвеєр, binding, серіалізацію, статуси | ✅ |
| End-to-end (реальний сервер, БД) | секунди | інфраструктуру, мережу | — |

### Підміна залежностей
`factory.WithWebHostBuilder(b => b.ConfigureServices(s => { ... }))` — можна
замінити `ICatalog` на фейк/мок, підкласти тестову БД, вимкнути авторизацію.

### Ізоляція стану
Тут `InMemoryCatalog` — Singleton, спільний для всіх тестів класу
(`IClassFixture`). Тести, що мутують дані, підбирають власні SKU й прибирають
за собою. Для суворої ізоляції: нова фабрика на тест, або скидання стану у
фікстурі, або окрема БД на тест.

### `xUnit` дрібниці
- `[Fact]` — тест без параметрів; `[Theory]` + `[InlineData]` — параметризований.
- `IClassFixture<T>` — один екземпляр `T` на всі тести класу.
- `async Task` тести підтримуються з коробки.

## Запуск

```bash
dotnet test tests/20.Testing
# або весь solution:
dotnet test
```

## Посилання

- Integration tests in ASP.NET Core: <https://learn.microsoft.com/aspnet/core/test/integration-tests>
- `WebApplicationFactory<TEntryPoint>`: <https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1>
- Unit testing best practices: <https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices>

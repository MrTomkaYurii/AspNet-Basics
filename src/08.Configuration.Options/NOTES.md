# 08. Конфігурація та патерн Options

## Що демонструємо

- Ланцюжок провайдерів конфігурації і правило перекриття.
- Прив'язку секції до типізованого класу + валідацію.
- Три способи «доставки» опцій: `IOptions` / `IOptionsSnapshot` / `IOptionsMonitor`.

## Ключові концепції

### Порядок джерел (кожне наступне перекриває попередні)
1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. User Secrets (тільки Development) — `dotnet user-secrets set "Catalog:Currency" "EUR"`
4. Змінні середовища — роздільник `__`: `Catalog__Currency=EUR`
5. Аргументи командного рядка — `--Catalog:Currency=EUR`

`(IConfigurationRoot)config).GetDebugView()` показує підсумкове значення кожного
ключа **і джерело**, звідки воно прийшло.

### Ієрархія та масиви
`Catalog:DefaultPageSize` — двокрапка як роздільник рівнів. У змінних середовища —
подвійне підкреслення. Масиви: `Catalog:Tags:0`, `Catalog:Tags:1`.

### Options: три інтерфейси

| Тип | Час життя | Перечитує зміни | Можна в middleware/singleton |
|---|---|---|---|
| `IOptions<T>` | Singleton | ні (обчислюється один раз) | так |
| `IOptionsSnapshot<T>` | Scoped | так, раз на запит | **ні** |
| `IOptionsMonitor<T>` | Singleton | так, миттєво + `OnChange` | так |

Порада: за замовчуванням `IOptions<T>`. `IOptionsSnapshot` — коли треба «гаряче»
оновлення в межах запиту. `IOptionsMonitor` — коли значення потрібне поза
запитом (фоновий сервіс, middleware).

### Валідація
```csharp
builder.Services.AddOptions<CatalogOptions>()
    .Bind(config.GetSection("Catalog"))
    .ValidateDataAnnotations()
    .Validate(o => o.DefaultPageSize <= 100, "PageSize завеликий")
    .ValidateOnStart();          // ← падіння на старті, а не при першому доступі
```
Без `ValidateOnStart()` помилкова конфігурація «вистрілить» лише тоді, коли
хтось уперше попросить опції — часто вже в проді.

## Спробуйте самі

1. `GET /config/sources` — подивіться список провайдерів і `debugView`.
2. `GET /options/io` — `EnableExperimentalSearch: true` (перекрито
   `appsettings.Development.json`).
3. `GET /options/snapshot`, потім **змініть `Currency` в `appsettings.json`** під
   час роботи застосунку, повторіть запит — значення оновиться без перезапуску.
   `GET /options/io` при цьому не зміниться.
4. Зламайте конфіг: поставте `"Currency": "usd"` (маленькі) → застосунок не
   стартує через `ValidateOnStart()` + `[RegularExpression]`.
5. Запустіть із `Catalog__Currency=EUR dotnet run` — змінна середовища перекриє файл.

## Посилання

- Configuration in ASP.NET Core: <https://learn.microsoft.com/aspnet/core/fundamentals/configuration/>
- Options pattern: <https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options>
- Options validation: <https://learn.microsoft.com/dotnet/core/extensions/options-library-authors#validate-options>
- Safe storage of app secrets: <https://learn.microsoft.com/aspnet/core/security/app-secrets>

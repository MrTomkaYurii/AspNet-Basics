# 11. Minimal API: прив'язка параметрів і валідація

## Що демонструємо

1. Звідки Minimal API бере значення для параметрів хендлера.
2. Дві стратегії валідації вводу: вбудована (.NET 10) та власний `endpoint filter`.

## Частина 1. Джерела прив'язки

| Джерело | Коли обирається за замовчуванням | Явний атрибут |
|---|---|---|
| Route values | ім'я параметра = сегмент `{...}` у шаблоні | `[FromRoute]` |
| Query string | простий тип, не в маршруті | `[FromQuery]` |
| Тіло (JSON) | складний тип | `[FromBody]` |
| Заголовок | — | `[FromHeader(Name = "...")]` |
| DI-контейнер | тип зареєстровано як сервіс | `[FromServices]` (необов'язково) |
| `HttpContext`, `HttpRequest`, `ClaimsPrincipal`, `CancellationToken` | завжди напряму | — |
| Форма | — | `[FromForm]`, `IFormFile` |

Додатково:
- **`string[]` / колекції** з query: `?tags=a&tags=b`.
- **`[AsParameters]`** — розкладає одну структуру на параметри з різних джерел.
- **Власні типи**: реалізуйте `IParsable<T>` (`TryParse`) для прив'язки з
  route/query або статичний `BindAsync` для повного контролю з `HttpContext`.
- Тіло можна прочитати лише **один раз** і лише для **одного** параметра.

## Частина 2. Валідація

### Вбудована (.NET 10)
```csharp
builder.Services.AddValidation();
```
Параметри-моделі з атрибутами `DataAnnotations` (`[Required]`, `[Range]`,
`[RegularExpression]`, `[StringLength]`, вкладені об'єкти через `[ValidatableType]`
/ source generator) перевіряються **до** виклику хендлера. Помилка →
`400 application/problem+json` з полем `errors` (формат `ValidationProblemDetails`).

### Endpoint filter (для складних правил)
`AddEndpointFilter` вклинюється навколо хендлера — можна перевіряти комбінації
полів, звірятися з БД, домальовувати помилки. Це «middleware рівня endpoint»:
бачить уже зв'язані аргументи (`ctx.GetArgument<T>(index)`).

### Валідація ≠ прив'язка
Якщо тип не зв'язався взагалі (напр. `int` отримав `"abc"`), це помилка
**прив'язки** → `400` ще раніше за валідацію, з іншим тілом.

## Спробуйте самі

1. `GET /bind/route/42`, `GET /bind/query?q=hub&page=2&tags=new&tags=sale`.
2. `GET /bind/header` із заголовком `X-Tenant: acme`; без нього → `400`.
3. `GET /bind/search/2?term=key&pageSize=5` — `[AsParameters]` з route + query.
4. `POST /validate` з `"name": "x", "price": 0` → `400 ValidationProblem`
   (жодного коду валідації в хендлері — це `AddValidation()`).
5. `POST /validate` з `"categoryId": 99` → `400` від endpoint filter (крос-польове правило).

## Посилання

- Parameter binding in minimal APIs: <https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/parameter-binding>
- Minimal API validation (.NET 10): <https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/validation>
- Filters in minimal API apps: <https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/min-api-filters>

# 11. Контролери: прив'язка параметрів і валідація

## Що демонструємо

1. Звідки MVC бере значення для параметрів дії.
2. Дві стратегії валідації: авто-`400` від `[ApiController]` за `DataAnnotations`
   і власна крос-польова перевірка через `ModelState`.

## Частина 1. Джерела прив'язки

| Джерело | Коли обирається за замовчуванням | Явний атрибут |
|---|---|---|
| Route values | ім'я параметра = сегмент `{...}` у шаблоні | `[FromRoute]` |
| Query string | простий тип, не в маршруті | `[FromQuery]` |
| Тіло (JSON) | складний тип (з `[ApiController]`) | `[FromBody]` |
| Заголовок | — | `[FromHeader(Name = "...")]` |
| DI-контейнер | тип зареєстровано як сервіс | `[FromServices]` |
| Форма | `IFormFile`, `[FromForm]` | `[FromForm]` |
| `HttpContext`, `CancellationToken`, `ClaimsPrincipal` | завжди напряму | — |

Додатково:
- **`string[]` / колекції** з query: `?tags=a&tags=b`.
- **Обов'язковість**: non-nullable простий параметр без значення за замовчуванням —
  обов'язковий (немає → `400`). `string?` / значення за замовчуванням — необов'язковий.
- Немає прямого аналога Minimal API `[AsParameters]`: у MVC або перелічують
  параметри дії, або роблять клас-модель, у якого на **властивостях** стоять
  `[FromRoute]` / `[FromQuery]` — тоді одна модель збирається з різних джерел.
- **Власні прості типи**: реалізуйте `IParsable<T>` (`TryParse`) для route/query.
- Тіло читається **один раз** і лише для **одного** параметра (`[FromBody]`).

### `[ApiController]` і вивід джерела
Без `[ApiController]` складний тип за замовчуванням теж збирається зі значень
маршруту/query. `[ApiController]` вмикає розумніший вивід: складний тип → `[FromBody]`,
`IFormFile` → форма, решта → route/query.

## Частина 2. Валідація

### Авто-`400` за `DataAnnotations`
`ProductInput` має `[Required]`, `[StringLength]`, `[Range]`. З `[ApiController]`
невалідна `ModelState` → відповідь `400 application/problem+json` з полем `errors`
(`ValidationProblemDetails`) **ще до тіла методу**. Жодного коду валідації для
простих правил писати не треба.

### Крос-польові правила — через `ModelState`
Правила, яких немає в атрибутах (звірка з БД, комбінації полів), перевіряють у
дії: `ModelState.AddModelError(поле, повідомлення)` + `return ValidationProblem(ModelState)`.
Формат помилки — той самий, клієнт обробляє його однаково.

### Валідація ≠ прив'язка
Якщо тип не зв'язався взагалі (`int` отримав `"abc"`) — це помилка **прив'язки**,
`400` ще раніше за валідацію, з іншим тілом.

### Де ще налаштовують валідацію
- Вимкнути авто-`400`: `builder.Services.Configure<ApiBehaviorOptions>(o => o.SuppressModelStateInvalidFilter = true)`.
- Власний формат відповіді: `o.InvalidModelStateResponseFactory = ...`.
- Складні правила як атрибути: власний `ValidationAttribute` або `IValidatableObject` на моделі.

## Спробуйте самі

1. `GET /bind/route/42`, `GET /bind/query?q=hub&page=2&tags=new&tags=sale`.
2. `GET /bind/header` із заголовком `X-Tenant: acme`; без нього → `400`.
3. `GET /bind/search/2?term=key&pageSize=5` — page з маршруту, решта з query.
4. `POST /validate` з `"name": "x", "price": 0` → `400 ValidationProblem`
   (авто-валідація `[ApiController]`, у дії нема жодної перевірки для цього).
5. `POST /validate` з `"categoryId": 99` → `400` від ручної помилки `ModelState`
   (крос-польове правило).

## Посилання

- Model binding in ASP.NET Core: <https://learn.microsoft.com/aspnet/core/mvc/models/model-binding>
- Model validation: <https://learn.microsoft.com/aspnet/core/mvc/models/validation>
- `[ApiController]` behaviors: <https://learn.microsoft.com/aspnet/core/web-api/#apicontroller-attribute>

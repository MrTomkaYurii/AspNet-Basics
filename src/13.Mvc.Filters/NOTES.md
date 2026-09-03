# 13. Фільтри MVC

## Що демонструємо

П'ять типів фільтрів, їхній порядок і чим фільтр відрізняється від middleware.

## Ключові концепції

### Конвеєр фільтрів
```
Authorization filter   — найраніше; пускає/не пускає (напр. [Authorize])
   Resource filter      — після авторизації, ДО model binding; кеш, short-circuit
      [model binding + валідація]
      Action filter      — навколо методу дії, бачить ActionArguments
         [ТІЛО ДІЇ]
      Action filter (executed)
   Exception filter      — ловить винятки з дії та action-фільтрів
   Result filter         — навколо виконання IActionResult (серіалізації)
   Resource filter (executed)
```

### Типи фільтрів

| Тип | Інтерфейс | Навіщо |
|---|---|---|
| Authorization | `IAuthorizationFilter` | доступ; не кидайте сюди бізнес-логіку |
| Resource | `IResourceFilter` | кеш, зміна форматерів, вимір, short-circuit до binding |
| Action | `IActionFilter` | валідація аргументів, лог, зміна результату |
| Exception | `IExceptionFilter` | єдина обробка винятків рівня MVC |
| Result | `IResultFilter` | «конверт» відповіді, заголовки, зміна `IActionResult` |

### Фільтр vs middleware

| | Middleware | Фільтр |
|---|---|---|
| Область | увесь конвеєр | лише MVC-запити |
| Знає про дію/модель | ні | так (`ActionDescriptor`, `ActionArguments`, `Result`) |
| Порядок | порядок реєстрації | тип + `Order` + scope (global/controller/action) |
| Доступ до DI | так | так (`[ServiceFilter]`, `[TypeFilter]`) |

Правило: наскрізне і не-MVC (логування всіх запитів, стиснення) → middleware;
специфічне для дій (конверт відповіді, валідація аргументів) → фільтр.

### Способи «повісити» фільтр
- Глобально: `AddControllers(o => o.Filters.Add<T>())`.
- На контролер/дію: атрибут `[TypeFilter(typeof(T))]`, `[ServiceFilter(typeof(T))]`,
  або власний атрибут, що є фільтром.
- Порядок у межах одного етапу: властивість `Order` (менше = раніше), потім scope.

### `[ServiceFilter]` vs `[TypeFilter]`
- `[ServiceFilter]` — екземпляр із DI (треба зареєструвати).
- `[TypeFilter]` — DI створює екземпляр, резолвлячи конструктор, реєстрація не потрібна;
  можна передати аргументи (`Arguments = [...]`).

## Спробуйте самі

1. `GET /api/demo/ok` → у полі `trace` видно фактичний порядок:
   `ResourceFilter.Executing → ActionFilter.Executing → Action body →
   ActionFilter.Executed → ResultFilter.Executing → ResourceFilter.Executed`.
2. `GET /api/demo/boom` → `500` від `ExceptionFilter`, у `trace` є крок `X`.
   Зверніть увагу: `ResultFilter` для нормального результату не спрацював.
3. `GET /api/demo/slow` → заголовок `X-Elapsed-Ms` від `ResourceFilter`.

## Посилання

- Filters in ASP.NET Core: <https://learn.microsoft.com/aspnet/core/mvc/controllers/filters>
- Filter order & scopes: <https://learn.microsoft.com/aspnet/core/mvc/controllers/filters#filter-scopes-and-the-order-of-execution>

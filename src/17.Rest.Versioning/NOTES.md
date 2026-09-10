# 17. REST: версіонування API

## Що демонструємо

Кілька версій контракту одночасно за допомогою `Asp.Versioning` та чотири
способи, якими клієнт вказує потрібну версію.

## Ключові концепції

### Навіщо версіонувати
Публічний API — це контракт. Зміна форми відповіді, видалення поля, інша
семантика — **breaking change**. Замість «зламати всіх» вводять нову версію, а
стару якийсь час підтримують і позначають застарілою.

### Чотири способи вказати версію

| Спосіб | Приклад | Плюс | Мінус |
|---|---|---|---|
| Сегмент URL | `/v2/products` | видно, кешується, просто | URI ресурсу «стрибає» між версіями |
| Query string | `/products?api-version=2.0` | не чіпає шлях | легко забути, гірше кешується |
| Заголовок | `X-Api-Version: 2.0` | URI стабільний | «невидимо», важче тестувати вручну |
| Media type | `Accept: application/json;v=2.0` | найбільш «RESTful» | найскладніше для клієнтів |

`ApiVersionReader.Combine(...)` вмикає кілька одночасно.

### Оголошення версій на контролері
```csharp
[ApiVersion(1)] [ApiVersion(2)]
[Route("v{version:apiVersion}/products")]
public sealed class ProductsController : ControllerBase
{
    [HttpGet, MapToApiVersion(1)] public ... GetV1();
    [HttpGet, MapToApiVersion(2)] public ... GetV2();
}
```
`AddApiVersioning(...).AddMvc()` вмикає ці атрибути. `[ApiVersion(1, Deprecated = true)]` —
позначити версію застарілою. Сегмент `v{version:apiVersion}` у `[Route]` дає спосіб
«версія в URL»; без нього версію візьмуть читачі з query / заголовка / media-type.

### Схема версій
`Asp.Versioning` підтримує `major.minor`, дати (`2025-01-01`), статус
(`2.0-beta`). Тримайтеся чогось одного. Семантика: змінюйте **major** на
breaking change.

### Deprecation
`[ApiVersion(1, Deprecated = true)]` на контролері + `ReportApiVersions = true` →
у відповідь додаються заголовки:
```
api-supported-versions: 1.0, 2.0
api-deprecated-versions: 1.0
```
Клієнт бачить, що час мігрувати. Додатково — `Sunset`-заголовок (RFC 8594) з датою.

### Мінімізуйте розповзання версій
Не версіонуйте на кожну дрібницю. Адитивні зміни (нове необов'язкове поле, новий
endpoint) — не breaking, версію піднімати не треба. Версіонуйте контракт,
а не реалізацію.

## Спробуйте самі

1. `GET /v1/products` — товар «як є». `GET /v2/products` — замість `categoryId`
   поле `category` (назва).
2. `GET /products?api-version=2.0` — версія з query, без сегмента в URL.
3. `GET /products` із заголовком `X-Api-Version: 2.0`.
4. `GET /products` із `Accept: application/json;v=2.0`.
5. Подивіться заголовки відповіді: `api-supported-versions`.
6. `GET /v1/report` → заголовок `api-deprecated-versions: 1.0`.
7. `GET /v3/products` → `404` (такого маршруту немає; для незнайомої версії в
   query/заголовку буде `400 UnsupportedApiVersion`).

## Посилання

- Asp.Versioning (ASP.NET API Versioning): <https://github.com/dotnet/aspnet-api-versioning>
- MVC versioning приклади: <https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNetCore/Mvc>
- RFC 8594 — The Sunset HTTP Header: <https://www.rfc-editor.org/rfc/rfc8594>

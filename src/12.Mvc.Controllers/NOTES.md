# 12. MVC-контролери для Web API

## Що демонструємо

Приклад 10 показав CRUD на контролері «як є». Тут — розбір, що саме дає
`[ApiController]`: авто-`400`, вивід джерела прив'язки, `ProblemDetails`,
обов'язкова атрибутна маршрутизація, `ActionResult<T>` для OpenAPI.

## Ключові концепції

### `AddControllers()` + `MapControllers()`
Перше реєструє інфраструктуру MVC (активація, model binding, форматери,
ApiExplorer). Друге додає знайдені дії до таблиці маршрутів. Обидва —
поверх того самого конвеєра й DI.

### `[ApiController]` — що вмикає
- **Авто-`400`** з `ValidationProblemDetails`, коли `ModelState.IsValid == false`
  (ще до тіла методу).
- **Інференс джерела прив'язки**: складний тип → `[FromBody]`, простий → route/query,
  `IFormFile` → форма.
- **`ProblemDetails`** для проблемних статус-кодів.
- **Обов'язкова атрибутна маршрутизація** (`[Route]`, `[HttpGet]` з шаблоном).

### `ControllerBase` vs `Controller`
Для API успадковують `ControllerBase` (без підтримки Razor View). `Controller` —
коли потрібні уявлення (MVC у класичному сенсі).

### Типи повернення
| Тип | Коли |
|---|---|
| `ActionResult<T>` | 200 з `T` **або** інший результат (`NotFound()`, `BadRequest()`) |
| `IActionResult` | лише результати, без «типу успіху» для OpenAPI |
| `T` | завжди 200; помилки — лише через винятки/фільтри |

### `CreatedAtAction` / `CreatedAtRoute`
Повертає `201` і будує `Location` за **іменем дії/маршруту** — не конкатенацією.

### `ModelState`
Словник результатів прив'язки та валідації. Можна дописати помилку вручну
(`ModelState.AddModelError(...)`) і повернути `ValidationProblem(ModelState)`
для крос-польових правил.

### Опис контракту для OpenAPI
Атрибути `[ProducesResponseType<T>(код)]` / `[Produces("...")]` не впливають на
поведінку, але уточнюють, які коди й типи повертає дія — для Swagger / Scalar
(приклад 19). Тут навмисно опущені, щоб не захаращувати.

## Спробуйте самі

1. `GET /api/products`, `GET /api/products/1`, `GET /api/products/999` → `404`.
2. `POST /api/products` з невалідним тілом → авто-`400` `ValidationProblemDetails`
   (у контролері немає жодної перевірки для цього).
3. `POST /api/products` з `categoryId: 99` → `400` від ручної помилки `ModelState`.
4. Успішний `POST` → `201`, дивіться `Location`, потім `GET` за ним.
5. `GET /api/categories`, `GET /api/categories/2`.

## Посилання

- Create web APIs with ASP.NET Core: <https://learn.microsoft.com/aspnet/core/web-api/>
- `[ApiController]` attribute: <https://learn.microsoft.com/aspnet/core/web-api/#apicontroller-attribute>
- Routing to controller actions: <https://learn.microsoft.com/aspnet/core/mvc/controllers/routing>
- Handle requests with controllers: <https://learn.microsoft.com/aspnet/core/mvc/controllers/actions>

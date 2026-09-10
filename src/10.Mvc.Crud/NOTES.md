# 10. Повний CRUD на контролерах

## Що демонструємо

Класичний набір операцій над ресурсом «товар» — список, читання, створення,
повна заміна, видалення — на контролері MVC, із правильними кодами відповідей.

## Ключові концепції

### Контролер = ресурс
```csharp
[ApiController]
[Route("products")]
public sealed class ProductsController(ICatalog catalog) : ControllerBase
{
    [HttpGet]              public IEnumerable<Product> GetAll() => ...;   // GET /products
    [HttpGet("{id:int}")]  public ActionResult<Product> GetById(int id) => ...;
}
```
`[Route]` задає базовий шлях; кожна дія додає HTTP-метод і, за потреби, сегмент.
Залежності — у первинний конструктор класу.

### `AddControllers()` + `MapControllers()`
Перше реєструє інфраструктуру MVC (активація, model binding, форматери,
ApiExplorer). Друге додає знайдені дії до таблиці маршрутів. Той самий конвеєр і
DI, що й у Minimal API.

### Типи повернення

| Тип | Коли |
|---|---|
| `ActionResult<T>` | 200 з `T` **або** інший результат (`NotFound()`, `BadRequest()`) |
| `IActionResult` | лише результати, без «типу успіху» |
| `T` | завжди 200 |

### Коди відповідей CRUD

| Операція | Метод | Успіх | Якщо немає ресурсу |
|---|---|---|---|
| список | `GET /products` | `200` + масив | — |
| читання | `GET /products/{id}` | `200` + об'єкт | `404` |
| створення | `POST /products` | `201` + `Location` + тіло | — |
| заміна | `PUT /products/{id}` | `200` (тут) / `204` | `404` (або `201`, якщо upsert) |
| видалення | `DELETE /products/{id}` | `204` | `204` або `404` |

`201 Created` **обов'язково** з заголовком `Location`. `CreatedAtAction(nameof(GetById), …)`
будує його за **іменем дії**, а не конкатенацією рядків.

### `[ApiController]` вже дає базову валідацію
`ProductInput` має `DataAnnotations` (`[Required]`, `[Range]`…), тож невалідне тіло
→ автоматичний `400 ValidationProblemDetails` ще до входу в метод. У цьому прикладі
руками перевіряємо лише **бізнес-правило** (категорія існує?). Крос-польова
валідація та джерела прив'язки — приклад 11.

### Ідемпотентність (детально — приклад 14)
`GET`, `PUT`, `DELETE` — ідемпотентні: повторний однаковий виклик не змінює стан
додатково. Тому `DELETE` неіснуючого — це `204`, а не помилка. `POST` —
не ідемпотентний.

## Спробуйте самі (див. `.http`)

1. `GET /products` → список; `GET /products/1` → об'єкт; `GET /products/999` → `404`.
2. `POST /products` з коректним тілом → `201`, подивіться заголовок `Location`,
   потім `GET` за цим URL.
3. `POST /products` з `"categoryId": 99` → `400` (перевірка бізнес-правила).
4. `POST /products` з `"name": "x", "price": 0` → `400` (авто-валідація `[ApiController]`).
5. `PUT /products/1` → `200` з новою `version`. `PUT /products/999` → `404`.
6. `DELETE /products/2` → `204`. Повторіть → знову `204` (ідемпотентність).

## Посилання

- Create web APIs with ASP.NET Core: <https://learn.microsoft.com/aspnet/core/web-api/>
- Routing to controller actions: <https://learn.microsoft.com/aspnet/core/mvc/controllers/routing>
- Handle requests with controllers: <https://learn.microsoft.com/aspnet/core/mvc/controllers/actions>
- `CreatedAtAction`: <https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.createdataction>

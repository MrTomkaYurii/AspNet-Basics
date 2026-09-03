# 10. Minimal API: повний CRUD

## Що демонструємо

Класичний набір операцій над ресурсом «товар» у стилі Minimal API:
список, читання, створення, повна заміна, видалення — з правильними
кодами відповідей.

## Ключові концепції

### `MapGroup`
```csharp
var products = app.MapGroup("/products").WithTags("Products");
products.MapGet("/{id:int}", ...);   // фактично /products/{id}
```
Спільний префікс, метадані, фільтри й політики — в одному місці.

### `TypedResults` та union-типи
- `Results.Ok(x)` повертає `IResult` — зручно, але тип «розмитий».
- `TypedResults.Ok(x)` повертає конкретний `Ok<T>` — краще для тестів і для OpenAPI.
- `Results<Ok<Product>, NotFound>` як тип повернення хендлера — компілятор
  стежить, що ви повертаєте лише заявлені варіанти, а OpenAPI-документ отримує
  всі можливі коди без ручних атрибутів.

### Коди відповідей CRUD

| Операція | Метод | Успіх | Якщо немає ресурсу |
|---|---|---|---|
| список | `GET /products` | `200` + масив | — |
| читання | `GET /products/{id}` | `200` + об'єкт | `404` |
| створення | `POST /products` | `201` + `Location` + тіло | — |
| заміна | `PUT /products/{id}` | `200`/`204` | `404` (або `201`, якщо upsert) |
| видалення | `DELETE /products/{id}` | `204` | `204` або `404` |

`201 Created` **обов'язково** з заголовком `Location`, що вказує на новий ресурс.

### Ідемпотентність (детально — приклад 14)
`GET`, `PUT`, `DELETE` — ідемпотентні: повторний однаковий виклик не змінює стан
додатково. Тому `DELETE` неіснуючого — це `204`, а не помилка. `POST` —
не ідемпотентний (кожен виклик створює новий ресурс).

### Впровадження залежностей
Параметри хендлера, що не прив'язані до маршруту/тіла/query, беруться з DI:
`(int id, ICatalog catalog) => ...`. Явно позначати `[FromServices]` не потрібно.

## Спробуйте самі (див. `.http`)

1. `GET /products` → список; `GET /products/1` → об'єкт; `GET /products/999` → `404`.
2. `POST /products` з коректним тілом → `201`, подивіться заголовок `Location`,
   потім `GET` за цим URL.
3. `POST /products` з `"price": 0` або поганим `sku` → `400` + `ValidationProblem`.
4. `PUT /products/1` → `200` з новою `version`. `PUT /products/999` → `404`.
5. `DELETE /products/2` → `204`. Повторіть → знову `204` (ідемпотентність).

## Посилання

- Minimal APIs overview: <https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/overview>
- Route groups: <https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/route-handlers#route-groups>
- Create responses (`TypedResults`): <https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses>

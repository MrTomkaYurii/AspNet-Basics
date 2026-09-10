# 16. REST: колекції та помилки

## Що демонструємо

- Пагінація, сортування, фільтрація колекції — через query string.
- Конверт відповіді + дубль метаданих у заголовках (`X-Total-Count`, `Link`).
- Єдиний формат помилок: `ProblemDetails` / `ValidationProblemDetails`.

## Ключові концепції

### Параметри колекції — у query, не в шляху
```
GET /products?q=hub&category=accessories&minPrice=10&maxPrice=100&sort=-price&page=2&pageSize=5
```
Шлях (`/products`) — це ідентичність колекції. Все, що звужує/впорядковує вибірку,
— параметри операції → query.

### Пагінація: два підходи

| | Offset (page/pageSize) | Cursor (keyset) |
|---|---|---|
| Просто | ✅ | ❌ |
| Стабільність при вставках | ❌ (зсув) | ✅ |
| «Перейти на сторінку N» | ✅ | ❌ |
| Великі дані / нескінченний скрол | повільно (OFFSET) | швидко |

Тут — offset. Завжди задавайте **max pageSize** (тут 100), щоб клієнт не
попросив мільйон рядків.

### Конверт vs «голий масив»
- Голий масив (`[ {...}, {...} ]`) — чистіше, але нема куди покласти `total`, `page`.
- Конверт (`{ items: [...], pageNumber, pageSize, total, totalPages }`) — зручно клієнту.
- Компроміс: голий масив у тілі + метадані в заголовках (`X-Total-Count`, `Link`
  за RFC 8288) — так робить GitHub API. Тут показано `X-Total-Count`.

### Формат помилок
- `ValidationProblem(ModelState)` → `400`, `application/problem+json`, поле
  `errors` — словник `поле → [повідомлення]`.
- `Problem(title:, statusCode:, extensions:)` → будь-який 4xx/5xx
  з кастомними полями (тут `productId`).
- Один формат на весь API — клієнт пише обробку помилок **раз**.

### Сортування — білий список
Якщо додаєте сортування за полем із query — ніколи не підставляйте його напряму
в `OrderBy` за рефлексією. Тримайте перелік дозволених полів; невідоме → `400`.

## Спробуйте самі

1. `GET /products?page=1&pageSize=2` — тіло (конверт) і заголовок `X-Total-Count`.
2. `GET /products?page=2&pageSize=2` — наступна сторінка.
3. `GET /products?q=клав` — фільтр за назвою.
4. `GET /products?pageSize=999` → `400` `ValidationProblem`, поле `pageSize`.
5. `GET /products/999` → `404` `ProblemDetails` з полем `productId`.

## Посилання

- Microsoft REST API Guidelines — Collections, paging, filtering: <https://github.com/microsoft/api-guidelines/blob/vNext/azure/Guidelines.md#98-collections>
- RFC 8288 — Web Linking (`Link`): <https://www.rfc-editor.org/rfc/rfc8288>
- Handle errors / Problem Details: <https://learn.microsoft.com/aspnet/core/web-api/handle-errors>

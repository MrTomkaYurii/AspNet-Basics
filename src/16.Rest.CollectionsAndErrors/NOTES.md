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
- Конверт (`{ items: [...], page, pageSize, total, totalPages }`) — зручно клієнту.
- Компроміс: голий масив у тілі + метадані в заголовках `X-Total-Count`, `Link`
  (так робить GitHub API).

### Заголовок `Link` (RFC 8288)
```
Link: <...?page=1>; rel="first", <...?page=5>; rel="last",
      <...?page=1>; rel="prev", <...?page=3>; rel="next"
```
Клієнт «гортає» за `rel="next"`, не конструюючи URL сам.

### Формат помилок
- `Results.ValidationProblem(errors)` → `400`, `application/problem+json`, поле
  `errors` — словник `поле → [повідомлення]`.
- `Results.Problem(title, detail, statusCode, extensions)` → будь-який 4xx/5xx
  з кастомними полями (тут `productId`).
- Один формат на весь API — клієнт пише обробку помилок **раз**.

### Сортування — білий список
Ніколи не підставляйте поле сортування з query напряму в запит/`OrderBy` за
рефлексією. Тут — `AllowedSortFields`; невідоме поле → `400`.

## Спробуйте самі

1. `GET /products?sort=-price&pageSize=2&page=1` — подивіться тіло **і**
   заголовки `X-Total-Count`, `Link`.
2. `GET /products?page=2` за `Link: rel="next"` з попередньої відповіді.
3. `GET /products?category=accessories&maxPrice=50`.
4. `GET /products?pageSize=999` → `400` `ValidationProblem`, поле `pageSize`.
5. `GET /products?sort=secret` → `400`, перелік дозволених полів.
6. `GET /products/999` → `404` `ProblemDetails` з полем `productId`.

## Посилання

- Microsoft REST API Guidelines — Collections, paging, filtering: <https://github.com/microsoft/api-guidelines/blob/vNext/azure/Guidelines.md#98-collections>
- RFC 8288 — Web Linking (`Link`): <https://www.rfc-editor.org/rfc/rfc8288>
- Handle errors / Problem Details: <https://learn.microsoft.com/aspnet/core/web-api/handle-errors>

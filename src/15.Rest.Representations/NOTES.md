# 15. REST: представлення ресурсу

## Що демонструємо

- **Content negotiation** — один ресурс, кілька форматів (JSON, XML).
- Налаштування **System.Text.Json**.
- **Умовні запити**: `ETag` + `If-None-Match` (кеш) та `If-Match` (запобігання втраті змін).
- Заголовки кешування (`Cache-Control`).
- **PATCH**: JSON Patch (RFC 6902) і JSON Merge Patch (RFC 7386).

## Ключові концепції

### Ресурс ≠ представлення ≠ формат
- **Ресурс** — абстрактна сутність за URI.
- **Представлення** — конкретний знімок стану (тут: `ProductRepresentation`,
  окремий від доменного `Product`).
- **Формат** — кодування представлення (JSON / XML / …), обирається за `Accept`.

### Content negotiation
Клієнт: `Accept: application/xml`. Сервер обирає **output formatter**. Якщо
жоден не підходить і `ReturnHttpNotAcceptable = true` → `406 Not Acceptable`.
Для тіла запиту аналогічно працює `Content-Type` + **input formatter**
(`[Consumes(...)]` звужує прийнятні типи).

### ETag та умовні запити

| Заголовок запиту | Призначення | Успіх | Невдача |
|---|---|---|---|
| `If-None-Match: <etag>` | «віддай, лише якщо змінилось» (кеш) | `200` + тіло | `304 Not Modified` (без тіла) |
| `If-Match: <etag>` | «зміни, лише якщо не чіпали» (lost update) | `200`/`204` | `412 Precondition Failed` |

ETag: `W/"..."` — слабкий (семантична еквівалентність), `"..."` — сильний
(побайтова). Тут формуємо з `id` + `version`.

**Оптимістичне блокування**: `GET` повертає `ETag`; клієнт редагує; `PUT`/`PATCH`
з `If-Match`. Якщо між `GET` і `PUT` хтось інший змінив ресурс — `412`, і зміни
першого клієнта не затираються мовчки.

### PATCH: два формати

| | JSON Patch (RFC 6902) | JSON Merge Patch (RFC 7386) |
|---|---|---|
| `Content-Type` | `application/json-patch+json` | `application/merge-patch+json` |
| Тіло | **список операцій**: `[{"op":"replace","path":"/price","value":42}]` | **частковий об'єкт**: `{"price":42}` |
| Операції | add / remove / replace / move / copy / test | лише присвоєння; `null` = видалити поле |
| Масиви | точкові зміни за індексом | заміна цілком |
| Плюс | детермінований, є `test` для конкурентності | простий, читабельний |

### JSON Patch у .NET 10
Пакет `Microsoft.AspNetCore.JsonPatch.SystemTextJson` — на базі STJ, без
Newtonsoft. `JsonPatchDocument<T>` + `patch.ApplyTo(target, onError)`.

## Спробуйте самі

1. `GET /products/1` з `Accept: application/json`, потім з `Accept: application/xml`.
2. `GET /products/1` з `Accept: text/csv` → `406`.
3. `GET /products/1` → скопіюйте `ETag`; повторіть із `If-None-Match: <etag>` → `304`.
4. `PUT /products/1` без `If-Match` → `428`. З **застарілим** ETag → `412`.
   Зі свіжим → `200`, у відповіді новий `ETag`.
5. JSON Patch: `PATCH /products/1`, `Content-Type: application/json-patch+json`,
   тіло `[{"op":"replace","path":"/price","value":1234.50}]`.
6. Merge Patch: `PATCH /products/1`, `Content-Type: application/merge-patch+json`,
   тіло `{"price": 999}`.

## Посилання

- Format response data / content negotiation: <https://learn.microsoft.com/aspnet/core/web-api/advanced/formatting>
- Conditional requests (ETag): <https://developer.mozilla.org/docs/Web/HTTP/Conditional_requests>
- JsonPatch in ASP.NET Core: <https://learn.microsoft.com/aspnet/core/web-api/jsonpatch>
- RFC 9110 §8.8 (Validators/ETag), RFC 6902 (JSON Patch), RFC 7386 (Merge Patch)

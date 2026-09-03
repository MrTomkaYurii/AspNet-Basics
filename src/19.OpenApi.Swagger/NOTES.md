# 19. OpenAPI та Swagger UI

## Що демонструємо

- Генерацію документа **OpenAPI** вбудованими засобами .NET 10.
- **Swagger UI** як інтерактивну документацію.
- Збагачення документа: `Info`, теги, summary/description, приклади схем,
  трансформери.

## Ключові концепції

### OpenAPI ≠ Swagger
- **OpenAPI** — специфікація формату опису API (раніше звалася Swagger Spec).
- **Swagger UI / Swashbuckle / NSwag / Scalar** — інструменти навколо неї
  (UI, генерація коду, тощо).

### Що дає документ
- Інтерактивна документація (Swagger UI, Scalar, Redoc).
- Генерація клієнтських SDK (`openapi-generator`, `NSwag`, Kiota).
- Контрактні тести, моки, перевірки сумісності в CI.
- Імпорт у Postman / Insomnia / API-gateway.

### У .NET 10
```csharp
builder.Services.AddOpenApi();     // генератор документа
app.MapOpenApi();                  // віддає /openapi/{documentName}.json
```
Без сторонніх пакетів. Документ будується з таблиці endpoint-ів + метаданих.

### Джерела метаданих
| Що в документі | Звідки |
|---|---|
| шляхи, методи, параметри | таблиця маршрутів |
| схеми запиту/відповіді | типи параметрів і результату (`TypedResults`, `Results<...>`) |
| коди відповідей | union-типи результату, `Produces<T>()`, `[ProducesResponseType]` |
| summary / description | `.WithSummary()`, `.WithDescription()`, XML-коментарі `///` |
| теги (групування) | `.WithTags()` |
| приклади, `Info`, security | **трансформери** (`AddDocumentTransformer`, `AddSchemaTransformer`, `AddOperationTransformer`) |

### XML-коментарі
Увімкніть `<GenerateDocumentationFile>true</GenerateDocumentationFile>` — і `///`
над типами/методами потраплять у схеми та описи.

### Production
Документ зазвичай лишають увімкненим (він корисний), а от **Swagger UI** часто
вимикають поза Development або ховають за авторизацією.

## Спробуйте самі

1. `GET /openapi/v1.json` — сирий документ. Знайдіть `info`, `paths`, `components/schemas`.
2. Відкрийте `http://localhost:5019/swagger` — інтерактивний UI, спробуйте
   «Try it out» на `POST /products` (приклад тіла вже підставлено трансформером схеми).
3. Подивіться, як union-тип `Results<Ok<Product>, NotFound>` перетворився на
   коди `200` і `404` без жодного атрибута.
4. Змініть `.WithSummary(...)` в коді — оновіть сторінку, текст у UI зміниться.

## Посилання

- OpenAPI support in ASP.NET Core: <https://learn.microsoft.com/aspnet/core/fundamentals/openapi/aspnetcore-openapi>
- Customize the OpenAPI document (transformers): <https://learn.microsoft.com/aspnet/core/fundamentals/openapi/customize-openapi>
- Swagger UI: <https://swagger.io/tools/swagger-ui/>
- OpenAPI Specification: <https://spec.openapis.org/oas/latest.html>

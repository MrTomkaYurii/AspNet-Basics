# 19. OpenAPI, Swagger UI та Scalar

## Що демонструємо

- Генерацію документа **OpenAPI** вбудованими засобами .NET 10.
- **Два UI поверх одного документа** одночасно: Swagger UI (`/swagger`) і
  Scalar (`/scalar`) — обидва читають той самий `/openapi/v1.json`.
- Збагачення документа: `Info`, теги, summary/description, приклади схем,
  трансформери.

> Ключова думка: документ OpenAPI — це **джерело правди**, а переглядачів може
> бути скільки завгодно й паралельно. UI — це просто HTML+JS, що завантажує JSON.

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
builder.Services.AddOpenApi();          // генератор документа
app.MapOpenApi();                       // віддає /openapi/{documentName}.json

app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "v1"));   // пакет Swashbuckle.AspNetCore.SwaggerUI
app.MapScalarApiReference(o => o.WithOpenApiRoutePattern("/openapi/v1.json")); // пакет Scalar.AspNetCore
```
Генерація документа — без сторонніх пакетів. Кожен UI — окремий маленький пакет
**лише з фронтендом**; можна підключити один, обидва, або жодного (тоді
користуються сирим JSON чи зовнішнім переглядачем).

### Swagger UI vs Scalar vs Redoc

| | Swagger UI | Scalar | Redoc |
|---|---|---|---|
| Пакет | `Swashbuckle.AspNetCore.SwaggerUI` | `Scalar.AspNetCore` | немає офіційного для ASP.NET, підключають статикою |
| «Try it out» (виконати запит) | так | так | ні (тільки читання) |
| Вигляд | класичний, впізнаваний | сучасний, генерує сніпети (curl, C#, JS…) | документо-орієнтований, гарний для публічних API |
| .NET-шаблони | до .NET 8 включно | .NET 9+ пропонують саме його | — |
| OpenAPI 3.1 | так | так | так |

Немає «правильного» — це смак і аудиторія. У цьому прикладі підключені обидва
активних варіанти, щоб побачити різницю на тому самому API.

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
Документ зазвичай лишають увімкненим (він корисний), а от **UI** часто
вимикають поза Development або ховають за авторизацією.

## Спробуйте самі

1. `GET /openapi/v1.json` — сирий документ. Знайдіть `info`, `paths`, `components/schemas`.
2. Відкрийте `http://localhost:5019/` — коренева сторінка з вибором UI.
3. `http://localhost:5019/swagger` і `http://localhost:5019/scalar` — **той самий
   API у двох переглядачах одночасно**. Спробуйте «Try it out» / «Send» на
   `POST /products` (приклад тіла підставлено трансформером схеми).
4. У Scalar подивіться згенеровані сніпети коду (curl, C#, JavaScript…) для запиту.
5. Подивіться, як union-тип `Results<Ok<Product>, NotFound>` перетворився на
   коди `200` і `404` без жодного атрибута.
6. Змініть `.WithSummary(...)` в коді — оновіть **обидві** сторінки, текст зміниться в обох.

## Посилання

- OpenAPI support in ASP.NET Core: <https://learn.microsoft.com/aspnet/core/fundamentals/openapi/aspnetcore-openapi>
- Customize the OpenAPI document (transformers): <https://learn.microsoft.com/aspnet/core/fundamentals/openapi/customize-openapi>
- Swagger UI: <https://swagger.io/tools/swagger-ui/>
- OpenAPI Specification: <https://spec.openapis.org/oas/latest.html>

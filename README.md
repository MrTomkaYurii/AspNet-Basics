# AspNet-Basics — курс «Основи ASP.NET Core»

Навчальний solution: від «голого» веб-застосунку до повноцінного REST API.
Кожен приклад — **окремий проект**, який запускається самостійно (`dotnet run`),
з коментарями в коді та окремим файлом **`NOTES.md`** з теорією і посиланнями на
офіційну документацію.

> **Стек:** .NET 10 (LTS) · ASP.NET Core · C# 14
> **Предметна область** усіх прикладів однакова — каталог товарів (`Catalog`),
> щоб увага була на платформі, а не на бізнес-логіці. Дані зберігаються в пам'яті.

> 📊 **Лекція:** [`docs/`](docs/) — презентація `Основи ASP.NET Core — лекція.pptx` (91 слайд:
> платформа, HTTP, конвеєр, DI, REST, безпека — з діаграмами й таблицями). Повний
> пояснювальний текст — у нотатках доповідача під кожним слайдом і окремо як
> `Конспект — Основи ASP.NET Core.md`.

---

## Як користуватися

```bash
# зібрати весь курс
dotnet build

# запустити конкретний приклад
dotnet run --project src/03.Pipeline.FromScratch

# приклади мають файл *.http — відкрийте його у Visual Studio / VS Code (REST Client)
# і надсилайте запити прямо звідти
```

Кожен приклад слухає власний порт (`5001`, `5002`, … див. `Properties/launchSettings.json`),
тож кілька прикладів можна тримати запущеними одночасно.

### Документація API в кожному прикладі

Майже всі приклади (крім суто конвеєрного `03`) підключають **один OpenAPI-документ
і два переглядачі поверх нього** — через спільний хелпер `Common.ApiDocs`:

| URL | Що це |
|---|---|
| `/openapi/v1.json` | сам документ OpenAPI (джерело правди) |
| `/swagger` | Swagger UI (пакет Swashbuckle) |
| `/scalar` | Scalar (сучасний UI) |

Обидва UI працюють **паралельно** — це просто HTML+JS, що завантажують той самий JSON.

**Браузер при старті.** Visual Studio / Rider / `dotnet watch` відкривають сторінку
з `launchSettings.json` (`launchUrl`). Для звичайного `dotnet run` хелпер сам
відкриває потрібну сторінку (змінна `DEVDOCS_LAUNCH`: `"scalar"` для API-прикладів,
`""` — корінь для решти). Приклад `19` розглядає це все докладно й «руками».

### Структура кожного проекту

| Файл | Призначення |
|---|---|
| `Program.cs` (+ інші `.cs`) | Код прикладу з навчальними коментарями |
| `NOTES.md` | Теорія: що демонструємо, ключові концепції, чеклист, посилання на docs |
| `*.http` | Готові HTTP-запити для ручної перевірки (VS / Rider / VS Code + REST Client) |
| `Properties/launchSettings.json` | Профілі запуску, порт, стартова сторінка |

### Структура `Program.cs`

Кожен `Program.cs` поділений на три однакові фази життєвого циклу застосунку —
щоб було видно, що і в якому порядку відбувається:

```csharp
// ======================================================================
//  1 · СЕРВІСИ  (реєстрація в DI)          ← builder.Services.Add...
// ======================================================================

var app = builder.Build();

// ======================================================================
//  2 · КОНВЕЄР  (middleware)               ← app.Use...
// ======================================================================

// ======================================================================
//  3 · ЗАПИТИ  (endpoints / обробники)     ← app.Map... / app.MapControllers()
// ======================================================================

app.Run();
```

Якщо в прикладі якоїсь фази немає (напр. власних middleware), банер усе одно
лишається з приміткою — так видно канонічну структуру в усіх прикладах.

---

## Навчальний план

### Модуль 0. Спільна основа
| Проект | Тема |
|---|---|
| [`00.Common`](src/00.Common) | Доменні моделі каталогу + сховище в пам'яті. Підключається до решти прикладів. |

### Модуль 1. Хостинг і платформа
| Проект | Тема |
|---|---|
| [`01.Hosting.Minimal`](src/01.Hosting.Minimal) | Що таке Host, `WebApplication`, `builder` vs `app`, вбудовані DI / конфігурація / логування, коректне завершення роботи |
| [`02.Hosting.ServersAndEnvironment`](src/02.Hosting.ServersAndEnvironment) | Kestrel vs IIS / HTTP.sys, `launchSettings.json`, URL та порти, `IWebHostEnvironment`, середовища Development / Production |

### Модуль 2. Конвеєр обробки запиту (middleware)
| Проект | Тема |
|---|---|
| [`03.Pipeline.FromScratch`](src/03.Pipeline.FromScratch) | `app.Use` / `app.Run`, делегат `next`, порядок middleware, коротке замикання, `HttpContext` руками |
| [`04.Pipeline.CustomMiddleware`](src/04.Pipeline.CustomMiddleware) | Middleware за конвенцією vs `IMiddleware`; приклади: тайминг запиту, correlation id |
| [`05.Pipeline.CrossCutting`](src/05.Pipeline.CrossCutting) | Обробка винятків + `ProblemDetails`, статичні файли, CORS, типові помилки порядку реєстрації |
| [`06.Pipeline.Branching`](src/06.Pipeline.Branching) | `Map`, `MapWhen`, `UseWhen` — розгалуження конвеєра |

### Модуль 3. Dependency Injection і конфігурація
| Проект | Тема |
|---|---|
| [`07.DependencyInjection.Lifetimes`](src/07.DependencyInjection.Lifetimes) | `Transient` / `Scoped` / `Singleton` на живому прикладі, captive dependency, `IServiceScopeFactory` |
| [`08.Configuration.Options`](src/08.Configuration.Options) | `appsettings.*.json`, змінні середовища, user secrets, `IOptions` / `IOptionsSnapshot` / `IOptionsMonitor`, валідація опцій |

### Модуль 4. Маршрутизація
| Проект | Тема |
|---|---|
| [`09.Routing.Endpoints`](src/09.Routing.Endpoints) | Endpoint routing, шаблони й обмеження маршрутів, route vs query, `LinkGenerator`, місце маршрутизації в конвеєрі |

### Модуль 5. Web API на контролерах
| Проект | Тема |
|---|---|
| [`10.Mvc.Crud`](src/10.Mvc.Crud) | Повний CRUD на контролері, `ActionResult<T>`, `CreatedAtAction`, коди відповідей, `Location` |
| [`11.Mvc.BindingAndValidation`](src/11.Mvc.BindingAndValidation) | Джерела прив'язки (`[FromRoute]`/`[FromQuery]`/`[FromHeader]`/`[FromBody]`), авто-`400`, `ModelState` |

### Модуль 6. MVC / контролери (глибше)
| Проект | Тема |
|---|---|
| [`12.Mvc.Controllers`](src/12.Mvc.Controllers) | Розбір `[ApiController]`: авто-`400`, вивід джерела прив'язки, `ProblemDetails`, `ActionResult<T>` |
| [`13.Mvc.Filters`](src/13.Mvc.Filters) | Фільтри Authorization / Resource / Action / Exception / Result, порядок, відмінність від middleware |

### Модуль 7. REST по-справжньому
| Проект | Тема |
|---|---|
| [`14.Rest.ResourcesAndVerbs`](src/14.Rest.ResourcesAndVerbs) | Дизайн ресурсів та URI, семантика `GET/POST/PUT/PATCH/DELETE`, ідемпотентність, коди відповідей, `Location` |
| [`15.Rest.Representations`](src/15.Rest.Representations) | Content negotiation, налаштування JSON, `PATCH` (JSON Patch / merge), `ETag` + `If-Match`, заголовки кешування |
| [`16.Rest.CollectionsAndErrors`](src/16.Rest.CollectionsAndErrors) | Пагінація / сортування / фільтрація, конверт відповіді, `ProblemDetails` / `ValidationProblem` |
| [`17.Rest.Versioning`](src/17.Rest.Versioning) | `Asp.Versioning`: версія в URL / заголовку / media-type, deprecation |
| [`18.Rest.Hypermedia`](src/18.Rest.Hypermedia) | HATEOAS, посилання у відповіді, рівні зрілості за Річардсоном |

### Модуль 8. Документація і тести
| Проект | Тема |
|---|---|
| [`19.OpenApi.Swagger`](src/19.OpenApi.Swagger) | Вбудований OpenApi-документ .NET 10 + Swagger UI **та** Scalar паралельно, схеми, приклади, трансформери, XML-коментарі |
| [`20.Testing`](tests/20.Testing) | `WebApplicationFactory`, інтеграційні тести конвеєра та endpoint-ів |

### Модуль 9. Безпека (базово)
| Проект | Тема |
|---|---|
| [`21.Security.AuthBasics`](src/21.Security.AuthBasics) | Автентифікація vs авторизація, схеми, JWT bearer, `[Authorize]`, policy / role, місце auth у конвеєрі |

---

## Рекомендований порядок

1. **Модуль 1–2** дають головну ідею: *ASP.NET Core — це конвеєр middleware поверх хоста*.
2. **Модуль 4** показує, що маршрутизація — теж middleware.
3. **Модуль 5–6** — Web API на контролерах: CRUD, прив'язка й валідація, розбір `[ApiController]`, фільтри.
4. **Модуль 7** — семантика REST на вже знайомому API.
5. **Модулі 3, 8, 9** можна вставляти після Модуля 1 у будь-якому місці.

## Корисні посилання

- ASP.NET Core docs: <https://learn.microsoft.com/aspnet/core>
- Fundamentals → Middleware: <https://learn.microsoft.com/aspnet/core/fundamentals/middleware>
- Create web APIs with controllers: <https://learn.microsoft.com/aspnet/core/web-api/>
- RFC 9110 (HTTP Semantics): <https://www.rfc-editor.org/rfc/rfc9110>
- RFC 9457 (Problem Details): <https://www.rfc-editor.org/rfc/rfc9457>
- Microsoft REST API Guidelines: <https://github.com/microsoft/api-guidelines>

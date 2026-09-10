# 09. Endpoint routing

## Що демонструємо

Як ASP.NET Core зіставляє URL із обробником: шаблони маршрутів, обмеження
(constraints), значення за замовчуванням, catch-all, пріоритет маршрутів,
route values проти query string, генерацію посилань.

## Ключові концепції

### Два кроки маршрутизації
```
UseRouting()    → обирає Endpoint, кладе його в HttpContext (context.GetEndpoint())
   ... middleware тут уже знає обраний endpoint (напр. авторизація за метаданими)
UseEndpoints()  → виконує обраний endpoint
```
`MapControllers()` (як і Minimal API) додає обидва виклики автоматично. Явними
вони потрібні лише коли треба вклинити middleware **між** етапами — саме це
робить цей приклад заголовком `X-Matched-Endpoint`.

### Атрибутна маршрутизація контролерів
Шаблон, constraints і значення за замовчуванням задають атрибути дії:
`[HttpGet("/products/{id:int}")]`, `[HttpGet("/pages/{page:int=1}")]`,
`[HttpGet("/files/{**path}")]`. Провідний `/` робить шаблон абсолютним
(без префікса `[Route]` контролера). Ім'я маршруту — `[HttpGet("...", Name = "product")]`.

### Шаблон маршруту
```
/products/{id:int}/reviews/{reviewId:guid?}
         └ літерал  └ параметр + constraint  └ '?' = необов'язковий
{page:int=1}          — значення за замовчуванням
{**path}              — catch-all (захоплює слеші)
```

### Поширені constraints
`int`, `long`, `guid`, `bool`, `datetime`, `decimal`, `alpha`, `minlength(n)`,
`maxlength(n)`, `length(n)`, `min(n)`, `max(n)`, `range(a,b)`, `regex(...)`.
Constraint — це фільтр **співставлення**, а не валідація: несумісний сегмент дає
`404` (маршрут не підійшов), а не `400`.

### Пріоритет маршрутів
Конкретніший шаблон виграє: `/products/featured` перекриває `/products/{id}`.
За потреби — `[Route("...", Order = n)]` (менше = раніше). Неоднозначність двох
однаково специфічних маршрутів → виняток `AmbiguousMatchException`.

### Route values vs query string
- **Шлях** ідентифікує ресурс: `/products/42`, `/categories/laptops`.
- **Query** — параметри операції над колекцією: `?q=hub&page=2&sort=price`.
- Не кладіть у шлях те, що не є частиною ідентичності ресурсу.

### Генерація URL
`LinkGenerator` / `IUrlHelper` будують URL за **іменем** маршруту
(`[HttpGet("...", Name = "...")]`) і параметрами. Рядкова конкатенація URL —
джерело багів із PathBase, кодуванням і версіонуванням.

## Спробуйте самі

1. `GET /products/1` → товар. `GET /products/abc` → `404` (не пройшов `:int`).
2. `GET /products/featured` → літеральний маршрут виграв у `/products/{id}`.
3. `GET /products/1/reviews/3fa85f64-5717-4562-b3fc-2c963f66afa6` → два параметри.
4. `GET /categories/laptops` → ок; `GET /categories/123` → `404` (`:alpha`).
5. `GET /files/img/2026/logo.png` → catch-all зібрав увесь хвіст у `path`.
6. `GET /pages` → `page = 1` (значення за замовчуванням).
7. `GET /link/7` → готовий URL, зібраний `LinkGenerator`.
8. У будь-якій відповіді дивіться заголовок `X-Matched-Endpoint` — його додає
   middleware між `UseRouting` і виконанням.

## Посилання

- Routing in ASP.NET Core: <https://learn.microsoft.com/aspnet/core/fundamentals/routing>
- Route constraint reference: <https://learn.microsoft.com/aspnet/core/fundamentals/routing#route-constraint-reference>
- URL generation with LinkGenerator: <https://learn.microsoft.com/aspnet/core/fundamentals/routing#url-generation-with-linkgenerator>

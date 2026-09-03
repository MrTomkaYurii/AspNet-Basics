# 04. Власні middleware

## Що демонструємо

Два способи оформити middleware як клас, коли їх треба перевикористовувати або
покрити тестами:

| | Convention-based | Factory-based (`IMiddleware`) |
|---|---|---|
| Інтерфейс | немає | `IMiddleware` |
| Створення екземпляра | **один раз** на застосунок | **на кожен запит** |
| Реєстрація в DI | не потрібна | обов'язкова (`AddScoped<T>()`) |
| Scoped-залежності | лише параметром `InvokeAsync` | можна в конструктор |
| Сигнатура | `InvokeAsync(HttpContext)` | `InvokeAsync(HttpContext, RequestDelegate)` |

## Ключові концепції

### Пастка часу життя (captive dependency)
У convention-based middleware конструктор викликається **один раз**. Якщо взяти
туди Scoped- або Transient-сервіс, він «застрягне» на весь час життя застосунку
(фактично стане Singleton) з непередбачуваними наслідками. Правильно — брати такі
залежності параметром `InvokeAsync(HttpContext ctx, IMyScoped svc)`.
(Детально про час життя — приклад 07.)

### `Response.OnStarting`
Заголовки їдуть клієнту разом із першим байтом тіла. Після цього
`Response.Headers[...] = ...` мовчки нічого не зробить (або кине виняток).
`context.Response.OnStarting(callback)` реєструє колбек, який виконається
безпосередньо перед відправкою заголовків — саме там безпечно дописувати
`Server-Timing`, `X-Correlation-ID` тощо.

### `HttpContext.Items`
Словник, що живе рівно один запит. Зручно передавати дані між middleware та
endpoint (тут — correlation id).

### `ILogger.BeginScope`
Додає властивості до **всіх** записів логу в межах блоку `using`. З
структурованим логуванням (Seq, ELK, Application Insights) це дає змогу
відфільтрувати всі рядки одного запиту за `CorrelationId`.

### Метод-розширення `UseXxx()`
`app.UseMiddleware<T>()` працює і без обгортки, але окремий `UseRequestTiming()`
читабельніший і дає місце для параметрів/перевірок.

## Спробуйте самі

1. `GET /` — у відповіді подивіться заголовки `X-Correlation-ID`, `Server-Timing`.
2. Повторіть запит із власним заголовком `X-Correlation-ID: my-trace-1` — сервер
   його підхопить (наскрізне трасування).
3. `GET /slow` кілька разів — `Server-Timing` змінюється.
4. `GET /?boom=1` — виняток; зараз впаде некрасиво. Приклад 05 це виправляє.
5. Порівняйте логи: у кожному рядку є `CorrelationId` завдяки `BeginScope`.

## Посилання

- Write custom middleware: <https://learn.microsoft.com/aspnet/core/fundamentals/middleware/write>
- Factory-based middleware (`IMiddleware`): <https://learn.microsoft.com/aspnet/core/fundamentals/middleware/extensibility>
- Logging scopes: <https://learn.microsoft.com/aspnet/core/fundamentals/logging/#log-scopes>

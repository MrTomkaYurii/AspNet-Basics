# 04. Власні middleware

## Що демонструємо

Три способи оформити middleware і різницю між двома «класовими»:

| | Convention-based | `IMiddleware` |
|---|---|---|
| Інтерфейс | немає | `IMiddleware` |
| Створення екземпляра | **один раз** на застосунок | **на кожен запит** |
| Реєстрація в DI | не потрібна | обов'язкова (`AddScoped<T>()`) |
| Scoped-залежності | лише параметром `InvokeAsync` | можна в конструктор |
| Сигнатура | `InvokeAsync(HttpContext)` | `InvokeAsync(HttpContext, RequestDelegate)` |

Плюс третій, найпростіший спосіб — інлайн `app.Use(async (ctx, next) => …)`.

## Ключові концепції

### Пастка часу життя (captive dependency)
У convention-based middleware конструктор викликається **один раз**. Scoped/Transient-
сервіс, узятий у конструктор, «застрягає» на весь час життя застосунку. Такі
залежності беруть параметром `InvokeAsync(HttpContext ctx, IMyScoped svc)`.
`IMiddleware` цієї проблеми не має — новий екземпляр на запит. (Детально — приклад 07.)

### `HttpContext.Items`
Словник, що живе рівно один запит. Зручно передавати дані між middleware та
endpoint (тут — correlation id).

### Метод-розширення `UseXxx()`
`app.UseMiddleware<T>()` працює і без обгортки, але окремий `UseCorrelationId()`
читабельніший — саме так оформлені `UseRouting`, `UseCors`, `UseAuthentication`.

## Спробуйте самі

1. `GET /` — у відповіді заголовки `X-Correlation-ID`, `X-Powered-By`; у консолі — лог із часом.
2. Повторіть із власним `X-Correlation-ID: my-trace-1` — сервер його підхопить (наскрізне трасування).
3. Поміняйте місцями `app.UseCorrelationId()` і `app.UseRequestTiming()` — порядок у логах/заголовках зміниться.

## Посилання

- Write custom middleware: <https://learn.microsoft.com/aspnet/core/fundamentals/middleware/write>
- Factory-based middleware (`IMiddleware`): <https://learn.microsoft.com/aspnet/core/fundamentals/middleware/extensibility>

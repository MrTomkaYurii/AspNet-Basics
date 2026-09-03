# 05. Наскрізна функціональність та порядок middleware

## Що демонструємо

- Обробка необроблених винятків і єдиний формат помилок — **Problem Details** (RFC 9457).
- `UseStatusCodePages` — тіло для «голих» статус-кодів.
- Статичні файли з `wwwroot/`.
- **CORS** — коли браузерний JS звертається з іншого origin.
- **Порядок реєстрації middleware** — головна причина складновловимих багів.

## Канонічний порядок конвеєра

```
UseExceptionHandler / UseDeveloperExceptionPage   ← найперший: огортає все
UseHsts (prod)
UseHttpsRedirection
UseStaticFiles                                     ← до маршрутизації (short-circuit)
UseRouting                                         ← часто неявний
UseCors
UseAuthentication
UseAuthorization
UseSession / UseResponseCaching / ...
<ваші endpoint-и>
```

Типові помилки:
- `UseCors` **після** endpoint-ів → заголовки CORS не додаються.
- `UseAuthorization` **перед** `UseRouting` → немає інформації про endpoint, `[Authorize]` не працює як слід.
- `UseStaticFiles` **після** важкого middleware → статика проходить зайву роботу.
- `UseExceptionHandler` **не першим** → винятки у middleware над ним не ловляться.

## Problem Details (RFC 9457)

Стандартний формат тіла помилки, `Content-Type: application/problem+json`:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "instance": "/api/fail",
  "traceId": "00-...."
}
```
`AddProblemDetails()` вмикає генерацію такого тіла для помилок фреймворку;
`CustomizeProblemDetails` дає додати свої поля. Вручну — `Results.Problem(...)` /
`TypedResults.Problem(...)`.

## CORS у двох словах

Same-origin policy браузера блокує крос-доменні запити з JS. Сервер має явно
дозволити origin заголовком `Access-Control-Allow-Origin`. Для «непростих»
запитів браузер спершу шле `OPTIONS` (preflight). `AddCors` + `UseCors` +
іменована політика — стандартний шлях. CORS **не** захист сервера, а послаблення
обмеження браузера.

## Спробуйте самі

1. `dotnet run` (Development) → `GET /api/fail` — сторінка розробника зі стеком.
2. `dotnet run --launch-profile "Production-like"` → `GET /api/fail` —
   тепер `application/problem+json`, без стека.
3. `GET /api/missing` → 404 із текстовим тілом від `UseStatusCodePages`.
4. Відкрийте `http://localhost:5005/` — це `wwwroot/index.html`.
5. CORS: `curl -H "Origin: https://example.com" -i http://localhost:5005/api/data`
   → є `Access-Control-Allow-Origin`. З `Origin: https://evil.com` — немає.

## Посилання

- Handle errors in ASP.NET Core: <https://learn.microsoft.com/aspnet/core/fundamentals/error-handling>
- Problem Details service: <https://learn.microsoft.com/aspnet/core/web-api/handle-errors#problem-details-service>
- Middleware order: <https://learn.microsoft.com/aspnet/core/fundamentals/middleware/#middleware-order>
- Enable CORS: <https://learn.microsoft.com/aspnet/core/security/cors>
- RFC 9457 — Problem Details: <https://www.rfc-editor.org/rfc/rfc9457>

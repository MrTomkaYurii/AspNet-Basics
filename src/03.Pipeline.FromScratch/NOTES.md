# 03. Конвеєр обробки запиту «з нуля»

## Що демонструємо

Middleware у чистому вигляді: без маршрутизації, без контролерів. Порядок
реєстрації, делегат `next`, обробка «на вході» та «на виході», коротке замикання,
робота з `HttpContext.Request` / `HttpContext.Response` руками.

## Ключові концепції

### Конвеєр = вкладені делегати
```
app.Use(A);
app.Use(B);
app.Run(C);
```
розгортається у:
```
A( ctx, () => B( ctx, () => C(ctx) ) )
```
`await next(context)` — це виклик наступної «цибулини». Код **до** `next` іде на
вході, код **після** `next` — на виході, у зворотному порядку (LIFO).

### Три способи поводження middleware
| Поведінка | Як | Наслідок |
|---|---|---|
| Наскрізний | `await next(context)` | керування йде далі |
| Обробка входу/виходу | код до / після `next` | логування, заголовки, таймінг |
| Коротке замикання | не викликати `next` | endpoint не виконається (auth, кеш, rate-limit) |

### `app.Use` vs `app.Run`
- `app.Use(async (ctx, next) => …)` — звичайний middleware, *може* передати далі.
- `app.Run(async ctx => …)` — **термінальний**: `next` немає, конвеєр завершується тут.

### `HttpContext` — усе про запит
- `Request`: `Method`, `Path`, `Query`, `Headers`, `Body` (потік), `RouteValues`.
- `Response`: `StatusCode`, `Headers`, `Body`, `WriteAsync`, `WriteAsJsonAsync`.
- `context.Response.HasStarted` — після `true` заголовки й код відповіді
  змінювати **пізно** (вони вже пішли клієнту). Тому для «відкладених» заголовків
  використовують `Response.OnStarting(callback)`.

### Порядок вирішує все
Auth — раніше за endpoint. Обробка винятків — найпершою (щоб ловити все, що нижче).
Стиснення — до запису тіла. Помилки порядку — найпоширеніша причина «магічних» багів
у ASP.NET Core (див. приклад 05).

## Спробуйте самі

1. `GET /hello?name=Іван` — параметр із query, прочитаний руками.
2. `POST /echo` з тілом — читання `Request.Body` як потоку.
3. `GET /secret` без заголовка → `401` (коротке замикання в middleware #2).
4. `GET /secret` із `X-Api-Key: let-me-in` → проходить до кінця конвеєра.
5. `GET /nope` → `404` + дописаний рядок від middleware #3 (обробка на виході).
6. Подивіться заголовок `X-Request-Id` та лог із часом — їх додає middleware #1.

## Посилання

- ASP.NET Core Middleware: <https://learn.microsoft.com/aspnet/core/fundamentals/middleware/>
- Middleware order: <https://learn.microsoft.com/aspnet/core/fundamentals/middleware/#middleware-order>
- HttpContext: <https://learn.microsoft.com/aspnet/core/fundamentals/httpcontext>

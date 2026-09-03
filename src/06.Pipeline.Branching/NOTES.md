# 06. Розгалуження конвеєра

## Що демонструємо

Три способи дати частині запитів окремий набір middleware.

| Метод | Умова | Термінальна? | Чіпає `PathBase`? | Типове застосування |
|---|---|---|---|---|
| `Map(path, …)` | префікс шляху | так | так (обрізає префікс) | під-застосунок: `/admin`, `/health`, вебхуки |
| `MapWhen(pred, …)` | будь-яка | так | ні | гілка за заголовком / query / схемою |
| `UseWhen(pred, …)` | будь-яка | **ні** (повертається) | ні | умовний middleware: auth лише для `/api/*` |

## Ключові концепції

### Термінальна гілка (`Map`, `MapWhen`)
Потрапивши в гілку, запит **не повернеться** в основний конвеєр. Усе, що
зареєстровано в `app` після `Map`, для таких запитів не виконається. Гілка має
сама завершити відповідь (зазвичай `branch.Run(...)` або власні endpoint-и).

### `Map` і `PathBase`
`app.Map("/admin", ...)` для запиту `/admin/users` дає всередині гілки
`Request.Path == "/users"`, `Request.PathBase == "/admin"`. Генерація посилань
це враховує. `MapWhen` так не робить — `Path` лишається повним.

### `UseWhen` повертається
Якщо гілка `UseWhen` викликала `next`, керування **продовжиться** в основному
конвеєрі з того місця, де стояв `UseWhen`. Тому спільні endpoint-и, оголошені
після нього, спрацюють і для запитів, що потрапили в гілку.

### Не плутати з маршрутизацією
`Map*` — це низькорівневий поділ конвеєра. Для «звичайного» роутингу за
шаблонами (`/products/{id}`) використовують endpoint routing (приклад 09) та
`MapGet/MapControllers`. `Map(path, ...)` доречний для ізольованих під-систем.

## Спробуйте самі

1. `GET /admin` → 403 (немає `?token=root`). `GET /admin/x?token=root` →
   побачите, що внутрішній `Path` вже без `/admin`, а `PathBase` = `/admin`.
2. `GET /` із заголовком `X-Legacy-Client: 1` → потрапляє в legacy-гілку,
   спільна головна не виконується.
3. `GET /api/ping` без `X-Api-Key` → 401 (short-circuit у `UseWhen`).
4. `GET /api/ping` з `X-Api-Key: secret` → 200, і у відповіді є `X-Api-Auth: ok`
   — доказ, що `UseWhen` повернув керування в основні endpoint-и.
5. `GET /` — заголовок `X-Pipeline: main` є завжди (спільний middleware до розгалужень).

## Посилання

- Branch the middleware pipeline: <https://learn.microsoft.com/aspnet/core/fundamentals/middleware/#branch-the-middleware-pipeline>
- `Map`, `MapWhen`, `UseWhen`: <https://learn.microsoft.com/aspnet/core/fundamentals/middleware/#use-run-and-map>

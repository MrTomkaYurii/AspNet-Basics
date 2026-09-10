# 21. Основи безпеки: автентифікація та авторизація

## Що демонструємо

- Різницю **AuthN** (хто ти) і **AuthZ** (що тобі можна).
- Схему `Bearer` з валідацією JWT.
- `[Authorize]` / `[Authorize(Policy = "...")]` / `[AllowAnonymous]`, ролі, claims, політики.
- Місце auth-middleware у конвеєрі.

> ⚠️ Користувачі та ключ підпису зашиті в код — **лише для навчання**. У проді:
> справжній Identity Provider, ключ із секретів/Key Vault, обов'язковий HTTPS,
> короткі токени + refresh.

## Ключові концепції

### Дві різні речі

| | Автентифікація | Авторизація |
|---|---|---|
| Питання | «Хто робить запит?» | «Чи має він право?» |
| Результат | `HttpContext.User` (`ClaimsPrincipal`) | дозвіл / `401` / `403` |
| Middleware | `UseAuthentication()` | `UseAuthorization()` |
| Провал | `401 Unauthorized` (немає/невалідний токен) | `403 Forbidden` (є особа, немає прав) |

`401` ≠ `403`. `401` — «представся». `403` — «тебе впізнали, але зась».

### Порядок у конвеєрі
```
UseRouting()          // обрано endpoint і його метадані (зокрема [Authorize])
UseAuthentication()   // з Authorization: Bearer <jwt> будує User
UseAuthorization()    // звіряє вимоги endpoint з User
<endpoints>
```
`UseAuthorization` **після** `UseRouting` — інакше не знає, які вимоги в endpoint.

### JWT (JSON Web Token)
Три частини через крапку: `header.payload.signature` (base64url). Payload —
claims (`sub`, `role`, `exp`, власні). Підпис (тут HMAC-SHA256 симетричним
ключем; у проді часто RSA/ECDSA — асиметричний). Сервер **не зберігає** токен:
перевіряє підпис і термін. Токен = «перепустка на пред'явника» → тільки HTTPS,
короткий `exp`.

`TokenValidationParameters` — що саме перевіряти: issuer, audience, ключ, час життя.

### Claims, ролі, політики

| Механізм | Приклад | Коли |
|---|---|---|
| `[Authorize]` | будь-хто автентифікований | приватний ресурс |
| `RequireRole("admin")` | claim `role=admin` | грубий поділ |
| `RequireClaim("subscription","premium")` | довільний claim | фічі, тарифи |
| Політика + `RequireAssertion` / `IAuthorizationRequirement` | `age >= 18`, «власник ресурсу» | складні правила |

Політики іменують і перевикористовують; логіку тримають в одному місці, а не
розсипають `if` по хендлерах.

## Спробуйте самі

1. `GET /public` — працює без токена.
2. `GET /me` без токена → `401`.
3. `POST /token` `{ "username": "alice", "password": "password" }` → скопіюйте `access_token`.
4. `GET /me` з `Authorization: Bearer <token>` → ваші claims.
5. `GET /admin` з токеном **alice** → `200`; з токеном **bob** → `403` (немає ролі admin).
6. Зіпсуйте один символ у токені → `401` (підпис не збігається).

## Посилання

- Overview of ASP.NET Core authentication: <https://learn.microsoft.com/aspnet/core/security/authentication/>
- Introduction to authorization: <https://learn.microsoft.com/aspnet/core/security/authorization/introduction>
- Policy-based authorization: <https://learn.microsoft.com/aspnet/core/security/authorization/policies>
- JWT bearer authentication: <https://learn.microsoft.com/aspnet/core/security/authentication/configure-jwt-bearer-authentication>

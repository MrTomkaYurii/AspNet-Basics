# 01. Мінімальний хост ASP.NET Core

## Що демонструємо

Найменший осмислений веб-застосунок і його три складові: **Host**, **HTTP-сервер (Kestrel)**,
**конвеєр middleware**. Плюс — життєвий цикл процесу й коректне завершення.

## Ключові концепції

### Generic Host
`Host` — це загальний контейнер із «Microsoft.Extensions.*»: DI, конфігурація,
логування, `IHostedService` (фонові задачі). `WebApplication` — це надбудова над
хостом, що додає HTTP-сервер і маршрутизацію. Той самий хост використовують і
консольні воркери, і gRPC, і Blazor.

### Дві фази: `builder` → `app`
```
var builder = WebApplication.CreateBuilder(args);  // 1. КОНФІГУРУЄМО
//   builder.Services      — реєстрація сервісів у DI
//   builder.Configuration — джерела налаштувань
//   builder.Logging       — провайдери логів
var app = builder.Build();                          // ── межа: контейнер "запечатано"
//   app.Use... / app.Map... — БУДУЄМО конвеєр
app.Run();                                          // 2. ЗАПУСКАЄМО (блокує потік)
```
Спроба додати сервіс після `Build()` — виняток. Це навмисно: склад контейнера
має бути детермінованим.

### Що `CreateBuilder` дає безкоштовно
- Конфігурація: `appsettings.json` → `appsettings.{Environment}.json` →
  User Secrets (у Development) → змінні середовища → аргументи CLI (кожне джерело
  перекриває попередні).
- Логування: Console, Debug, EventSource.
- Kestrel як сервер + інтеграція з IIS.
- DI-контейнер (`Microsoft.Extensions.DependencyInjection`).

### Graceful shutdown
`IHostApplicationLifetime` має три токени: `ApplicationStarted`,
`ApplicationStopping`, `ApplicationStopped`. На `SIGINT`/`SIGTERM` хост:
1. Спрацьовує `ApplicationStopping`.
2. Сервер перестає приймати нові з'єднання, добиває поточні запити
   (до `ShutdownTimeout`, типово 30 с).
3. Викликаються `StopAsync` усіх `IHostedService`.
4. Спрацьовує `ApplicationStopped`, процес виходить.

## Спробуйте самі

1. `dotnet run` → `GET /env` → подивіться `EnvironmentName`.
2. `GET /products` — сервіс `ICatalog` прийшов із DI-контейнера.
3. `GET /shutdown` (або `Ctrl+C`) — простежте порядок повідомлень у консолі:
   «Зупинка…» → «зупинено».
4. Запустіть із `ASPNETCORE_ENVIRONMENT=Production` — `GET /env` покаже інше середовище.

## Посилання

- .NET Generic Host: <https://learn.microsoft.com/dotnet/core/extensions/generic-host>
- WebApplication and WebApplicationBuilder: <https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/webapplication>
- App startup: <https://learn.microsoft.com/aspnet/core/fundamentals/startup>
- Host shutdown: <https://learn.microsoft.com/aspnet/core/fundamentals/host/generic-host#host-shutdown>

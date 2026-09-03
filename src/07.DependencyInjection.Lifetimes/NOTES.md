# 07. Час життя сервісів у DI

## Що демонструємо

Різницю між `Transient`, `Scoped`, `Singleton` на живому прикладі: кожен сервіс
має `Guid Id`, і ми порівнюємо ці Id, отримані в різних місцях одного запиту та
між запитами.

## Ключові концепції

| Час життя | Коли створюється | Скільки живе | Приклади |
|---|---|---|---|
| **Transient** | при кожному резолві | до GC | легкі stateless-хелпери, мапери |
| **Scoped** | раз на область (= HTTP-запит) | до кінця запиту | `DbContext`, репозиторії, «одиниця роботи» |
| **Singleton** | раз на застосунок | до зупинки процесу | кеш, конфіг, пул з'єднань, `HttpClient`-фабрика |

### «Область» (scope)
ASP.NET Core створює нову DI-область на **кожен HTTP-запит** і звільняє її в
кінці. `HttpContext.RequestServices` резолвить саме з цієї області. Тому Scoped =
«один на запит».

### Captive dependency (полонена залежність)
Якщо Singleton залежить від Scoped/Transient, то «коротший» сервіс живе стільки ж,
скільки Singleton — фактично стає Singleton. Наслідки: витоки, гонки, застарілі
дані, `ObjectDisposedException` на `DbContext`.

.NET захищає від цього: у Development хост валідує області на старті
(`ValidateScopes = true`) і **кидає виняток**
`Cannot consume scoped service 'X' from singleton 'Y'`. Розкоментуйте
`BrokenSingleton` у коді, щоб побачити.

### Як Singleton коректно користується Scoped
Впровадити `IServiceScopeFactory`, у потрібний момент `CreateScope()` і резолвити
з `scope.ServiceProvider` (див. endpoint `/manual-scope`). Так само роблять
`BackgroundService` та `IHostedService`.

### Антипатерн: `BuildServiceProvider()` у `Program.cs`
Створює **другий** контейнер → дублі синглтонів, витоки. Якщо потрібне значення на
етапі старту — використовуйте `builder.Configuration` або опції.

## Спробуйте самі

1. `GET /ids` — `transientA ≠ transientB`; `scoped == scopedFromMiddleware`;
   `singleton` скрізь однаковий.
2. Перезавантажте `GET /ids` — `scoped` змінився, `singleton` — ні.
3. `GET /scopes` — дві області → два різних Scoped Id.
4. Спробуйте зареєструвати сервіс, що бере `IScopedOperation` у конструктор, як
   `Singleton` — застосунок впаде на старті: `Cannot consume scoped service …`.

## Посилання

- Dependency injection: <https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection>
- Service lifetimes: <https://learn.microsoft.com/dotnet/core/extensions/dependency-injection#service-lifetimes>
- Scoped services in singletons / background tasks: <https://learn.microsoft.com/dotnet/core/extensions/scoped-service>
- DI guidelines & anti-patterns: <https://learn.microsoft.com/dotnet/core/extensions/dependency-injection-guidelines>

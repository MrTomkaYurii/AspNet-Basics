# 02. HTTP-сервери та середовища виконання

## Що демонструємо

- Хто фізично приймає HTTP-запит і як його налаштувати (**Kestrel**).
- Що таке **середовище** (`Development` / `Staging` / `Production` / власне) і як
  код на нього реагує.
- Звідки беруться URL та порти (`launchSettings.json`, `ASPNETCORE_URLS`, `Kestrel:Endpoints`).

## Ключові концепції

### Моделі хостингу
| Сервер | Платформа | Коли |
|---|---|---|
| **Kestrel** | крос-платформа | За замовчуванням. Або сам «дивиться в мережу», або за реверс-проксі. |
| **IIS (in-process)** | Windows | `AspNetCoreModuleV2` хостить застосунок усередині `w3wp.exe`. |
| **IIS (out-of-process)** | Windows | IIS як проксі до Kestrel. |
| **HTTP.sys** | Windows | Потрібні Windows-автентифікація, port sharing, kernel-mode. |

### Пріоритет джерел URL (від нижчого до вищого)
1. `Kestrel:Endpoints` в `appsettings.json`
2. `ASPNETCORE_URLS` / `--urls`
3. `app.Urls.Add(...)` / `UseUrls(...)` у коді
4. `launchSettings.json` `applicationUrl` (лише при `dotnet run` локально)

### Середовище
- Береться зі змінної **`ASPNETCORE_ENVIRONMENT`** (за замовчуванням `Production`).
- `launchSettings.json` задає її лише для локального запуску; на сервері —
  системна змінна або конфіг оркестратора.
- `env.IsDevelopment()`, `env.IsEnvironment("Testing")` — розгалуження поведінки.
- Головне правило: **детальні помилки, Swagger, seed-дані — тільки не в Production**.

### `launchSettings.json`
Файл для розробника, **не потрапляє в опубліковану збірку**. Кожен профіль —
окремий набір змінних середовища та URL. `dotnet run --launch-profile "Production-like"`.

## Спробуйте самі

1. `dotnet run` → `GET /env` (Development) і `GET /config` → `ExperimentalSearch: true`.
2. `dotnet run --launch-profile "Production-like"` → ті самі запити:
   середовище `Production`, `ExperimentalSearch: false` (перекрито
   `appsettings.Production.json`), а `/dev/ping` зникає (404).
3. `GET /server` — побачите `KestrelServer` і реальні адреси прослуховування.
4. Спробуйте `ASPNETCORE_URLS=http://localhost:5555 dotnet run` — порт зміниться,
   попри `launchSettings.json`.

## Посилання

- Kestrel web server: <https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel>
- Configure endpoints for Kestrel: <https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/endpoints>
- Use multiple environments: <https://learn.microsoft.com/aspnet/core/fundamentals/environments>
- Host and deploy: <https://learn.microsoft.com/aspnet/core/host-and-deploy/>

using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace Common;

/// <summary>
/// Зручність курсу: у КОЖНОМУ прикладі одним рядком підключити OpenAPI-документ
/// і одразу два переглядачі поверх нього:
///   • Swagger UI — <c>/swagger</c>
///   • Scalar     — <c>/scalar</c>
///
/// Обидва читають ОДИН документ <c>/openapi/v1.json</c> і працюють паралельно,
/// не заважаючи один одному: UI — це просто HTML+JS, що завантажує той самий JSON.
///
/// Приклад 19 навмисно робить те саме «руками» й детально — щоб показати механізм.
/// </summary>
public static class ApiDocs
{
    /// <summary>Викликати в секції «СЕРВІСИ».</summary>
    public static IServiceCollection AddApiDocs(this IServiceCollection services)
        => services.AddOpenApi();

    /// <summary>Викликати в секції «КОНВЕЄР» (після <c>builder.Build()</c>).</summary>
    public static WebApplication MapApiDocs(this WebApplication app)
    {
        app.MapOpenApi();                                                              // /openapi/v1.json
        app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "API v1"));         // /swagger
        app.MapScalarApiReference(o => o.WithOpenApiRoutePattern("/openapi/v1.json"));  // /scalar

        app.OpenBrowserOnStart();
        return app;
    }

    /// <summary>
    /// Відкриває потрібну сторінку в браузері при <c>dotnet run</c>.
    /// Visual Studio / Rider / <c>dotnet watch</c> роблять це самі (launchSettings.json),
    /// тому там ми не втручаємось, щоб не було двох вкладок.
    ///
    /// Яку саме сторінку відкрити — задає змінна середовища <c>DEVDOCS_LAUNCH</c>
    /// у launchSettings.json ("scalar", "swagger", "" = корінь). Немає змінної —
    /// нічого не відкриваємо (напр. під час тестів).
    /// </summary>
    public static WebApplication OpenBrowserOnStart(this WebApplication app)
    {
        var target = app.Configuration["DEVDOCS_LAUNCH"];
        if (target is null || !app.Environment.IsDevelopment()) return app;
        if (Environment.GetEnvironmentVariable("DOTNET_WATCH") is not null) return app;   // watch має власний browser-refresh
        if (Environment.GetEnvironmentVariable("VSAPPIDNAME") is not null) return app;    // запуск із Visual Studio

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var baseUrl = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault();
            if (baseUrl is null) return;

            var url = $"{baseUrl.TrimEnd('/')}/{target.TrimStart('/')}";
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch
            {
                // Немає браузера (headless / CI) — не критично, просто пропускаємо.
            }
        });

        return app;
    }
}

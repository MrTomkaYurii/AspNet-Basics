using Asp.Versioning;
using Common.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Rest.Versioning.Controllers;

/// <summary>
/// Один контролер обслуговує обидві версії. Сегмент <c>v{version:apiVersion}</c>
/// у шаблоні + <c>[MapToApiVersion]</c> на дії розводять запити за версією.
/// </summary>
[ApiController]
[ApiVersion(1)]
[ApiVersion(2)]
[Route("v{version:apiVersion}/products")]
public sealed class ProductsController(ICatalog catalog) : ControllerBase
{
    // v1 — товар «як є».
    [HttpGet]
    [MapToApiVersion(1)]
    public IEnumerable<Product> GetV1() => catalog.GetProducts();

    // v2 — змінений контракт: замість categoryId віддаємо назву категорії.
    [HttpGet]
    [MapToApiVersion(2)]
    public IEnumerable<object> GetV2() =>
        catalog.GetProducts().Select(p => new
        {
            p.Id,
            p.Name,
            p.Price,
            Category = catalog.FindCategory(p.CategoryId)?.Name,
        });
}

/// <summary>
/// Той самий ресурс без сегмента версії в URL — версію беремо з query
/// (<c>?api-version=2.0</c>), заголовка (<c>X-Api-Version</c>) або media-type
/// (<c>Accept: application/json;v=2.0</c>).
/// </summary>
[ApiController]
[ApiVersion(1)]
[ApiVersion(2)]
[Route("products")]
public sealed class ProductsNeutralController : ControllerBase
{
    [HttpGet]
    public object Get()
    {
        var requested = HttpContext.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;
        return new { version = requested?.ToString() ?? "1.0" };
    }
}

/// <summary>Застаріла версія → у відповіді заголовок <c>api-deprecated-versions</c>.</summary>
[ApiController]
[ApiVersion(1, Deprecated = true)]
[Route("v{version:apiVersion}/report")]
public sealed class ReportController : ControllerBase
{
    [HttpGet]
    public string Get() => "стара форма звіту";
}

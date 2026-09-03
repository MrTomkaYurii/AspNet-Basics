using Common.Domain;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Rest.Representations.Controllers;

[ApiController]
[Route("products")]
public sealed class ProductsController(ICatalog catalog) : ControllerBase
{
    // ── GET item з ETag та підтримкою умовних запитів ─────────────────────
    // Accept: application/json (типово) або application/xml → те саме представлення,
    // різне кодування (content negotiation робить MVC автоматично).
    [HttpGet("{id:int}")]
    [Produces("application/json", "application/xml")]
    public ActionResult<ProductRepresentation> Get(int id)
    {
        if (catalog.FindProduct(id) is not { } product)
            return NotFound();

        var etag = MakeETag(product);

        // If-None-Match: клієнт має кешовану копію. Якщо ETag збігся — 304, без тіла.
        var inm = Request.Headers.IfNoneMatch.ToString();
        if (!string.IsNullOrEmpty(inm) && inm == etag)
            return StatusCode(StatusCodes.Status304NotModified);

        Response.Headers.ETag = etag;
        Response.GetTypedHeaders().CacheControl = new()
        {
            Private = true,
            MaxAge = TimeSpan.FromSeconds(30),
        };

        return ProductRepresentation.From(product);
    }

    // ── PUT з If-Match → оптимістичне блокування ──────────────────────────
    // Клієнт зобов'язаний прислати ETag версії, яку він редагував. Якщо на
    // сервері вже інша версія — 412 Precondition Failed, зміни не втрачаються «мовчки».
    [HttpPut("{id:int}")]
    public ActionResult<ProductRepresentation> Put(int id, [FromBody] ProductRepresentation body)
    {
        if (catalog.FindProduct(id) is not { } current)
            return NotFound();

        var ifMatch = Request.Headers.IfMatch.ToString();
        if (string.IsNullOrEmpty(ifMatch))
            return Problem("Потрібен заголовок If-Match з поточним ETag.", statusCode: StatusCodes.Status428PreconditionRequired);

        if (ifMatch != MakeETag(current))
            return Problem("Ресурс змінено іншим клієнтом. Отримайте свіжу версію і повторіть.",
                statusCode: StatusCodes.Status412PreconditionFailed);

        var updated = catalog.Replace(id, body.ToInput())!;
        Response.Headers.ETag = MakeETag(updated);
        return ProductRepresentation.From(updated);
    }

    // ── PATCH: JSON Patch (RFC 6902), Content-Type: application/json-patch+json ──
    // Тіло — СПИСОК операцій: [{ "op": "replace", "path": "/price", "value": 42 }]
    [HttpPatch("{id:int}")]
    [Consumes("application/json-patch+json")]
    public ActionResult<ProductRepresentation> PatchJsonPatch(int id, [FromBody] JsonPatchDocument<ProductRepresentation> patch)
    {
        if (catalog.FindProduct(id) is not { } current)
            return NotFound();

        var draft = ProductRepresentation.From(current);
        patch.ApplyTo(draft, error => ModelState.AddModelError(
            error.AffectedObject?.GetType().Name ?? "patch", error.ErrorMessage));
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var updated = catalog.Replace(id, draft.ToInput())!;
        return ProductRepresentation.From(updated);
    }

    // ── PATCH: JSON Merge Patch (RFC 7386), Content-Type: application/merge-patch+json ──
    // Тіло — «частковий об'єкт»: { "price": 42 }. null означає «видалити поле».
    [HttpPatch("{id:int}")]
    [Consumes("application/merge-patch+json")]
    public ActionResult<ProductRepresentation> PatchMerge(int id, [FromBody] MergePatchProduct body)
    {
        if (catalog.FindProduct(id) is not { } current)
            return NotFound();

        var draft = ProductRepresentation.From(current);
        if (body.Name is not null) draft.Name = body.Name;
        if (body.Description is not null) draft.Description = body.Description;
        if (body.Price is not null) draft.Price = body.Price.Value;
        if (body.CategoryId is not null) draft.CategoryId = body.CategoryId.Value;

        var updated = catalog.Replace(id, draft.ToInput())!;
        return ProductRepresentation.From(updated);
    }

    // Слабкий ETag на основі версії ресурсу.
    private static string MakeETag(Product p) => $"W/\"{p.Id}-{p.Version}\"";
}

/// <summary>
/// Модель для JSON Merge Patch: усі поля необов'язкові (nullable).
/// Спрощення: тут «поле відсутнє» і «поле = null» не розрізняються, тож
/// видалення поля через merge patch не показано. Повна реалізація RFC 7386
/// вимагає тримати «сирий» JSON і перевіряти наявність ключа.
/// </summary>
public sealed class MergePatchProduct
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? CategoryId { get; set; }
}

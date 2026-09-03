using Common.Domain;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;

namespace Rest.Representations.Controllers;

[ApiController]
[Route("products")]
public sealed class ProductsController(ICatalog catalog) : ControllerBase
{
    // GET item. Формат (JSON / XML) обирає MVC за заголовком Accept.
    // ETag + If-None-Match: якщо у клієнта свіжа копія — 304 без тіла.
    [HttpGet("{id:int}")]
    public ActionResult<ProductRepresentation> Get(int id)
    {
        if (catalog.FindProduct(id) is not { } product)
            return NotFound();

        var etag = ETag(product);
        if (Request.Headers.IfNoneMatch == etag)
            return StatusCode(StatusCodes.Status304NotModified);

        Response.Headers.ETag = etag;
        Response.Headers.CacheControl = "private, max-age=30";
        return ProductRepresentation.From(product);
    }

    // PUT з If-Match → оптимістичне блокування: якщо на сервері вже інша версія,
    // повертаємо 412, і зміни першого клієнта не затираються «мовчки».
    [HttpPut("{id:int}")]
    public ActionResult<ProductRepresentation> Put(int id, ProductRepresentation body)
    {
        if (catalog.FindProduct(id) is not { } current)
            return NotFound();

        if (Request.Headers.IfMatch.Count == 0)
            return Problem("Потрібен заголовок If-Match.", statusCode: StatusCodes.Status428PreconditionRequired);

        if (Request.Headers.IfMatch != ETag(current))
            return Problem("Ресурс змінено іншим клієнтом.", statusCode: StatusCodes.Status412PreconditionFailed);

        var updated = catalog.Replace(id, body.ToInput())!;
        Response.Headers.ETag = ETag(updated);
        return ProductRepresentation.From(updated);
    }

    // PATCH: JSON Patch (RFC 6902). Тіло — список операцій:
    // [{ "op": "replace", "path": "/price", "value": 42 }]
    [HttpPatch("{id:int}")]
    [Consumes("application/json-patch+json")]
    public ActionResult<ProductRepresentation> Patch(int id, JsonPatchDocument<ProductRepresentation> patch)
    {
        if (catalog.FindProduct(id) is not { } current)
            return NotFound();

        var draft = ProductRepresentation.From(current);
        patch.ApplyTo(draft);
        var updated = catalog.Replace(id, draft.ToInput())!;
        return ProductRepresentation.From(updated);
    }

    // Слабкий ETag на основі версії ресурсу.
    private static string ETag(Product p) => $"W/\"{p.Id}-{p.Version}\"";
}

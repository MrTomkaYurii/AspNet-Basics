using Common.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Mvc.BindingAndValidation.Controllers;

/// <summary>
/// ЧАСТИНА 2. Валідація.
///
///   • <c>[ApiController]</c> + <c>DataAnnotations</c> на <see cref="ProductInput"/>
///     → автоматичний <c>400 ValidationProblemDetails</c> ДО входу в метод
///     (жодного рядка коду валідації для простих правил);
///   • крос-польові правила (яких немає в атрибутах) — вручну через
///     <see cref="ControllerBase.ModelState"/> + <c>ValidationProblem(...)</c>.
/// </summary>
[ApiController]
[Route("validate")]
public sealed class ValidateController(ICatalog catalog) : ControllerBase
{
    [HttpPost]
    public ActionResult<string> Post(ProductInput input)
    {
        // Сюди потрапляємо, лише якщо ModelState вже валідна (прості правила пройшли).
        if (catalog.FindCategory(input.CategoryId) is null)
        {
            ModelState.AddModelError(nameof(input.CategoryId), "Категорії з таким Id немає.");
            return ValidationProblem(ModelState);
        }

        return $"OK: {input.Name}";
    }
}

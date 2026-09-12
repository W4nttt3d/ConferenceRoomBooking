using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ConferenceRoomBooking.Api.Filters;

/// <summary>
/// Автоматично валідує вхідні DTO контролерів через FluentValidation, якщо
/// для типу аргументу зареєстровано IValidator&lt;T&gt;. Свідоме рішення:
/// написаний вручну фільтр замість пакета FluentValidation.AspNetCore
/// (автоматична MVC-інтеграція від FluentValidation офіційно визнана
/// застарілою автором бібліотеки) — так поведінка лишається явною
/// і легко тестованою, без прихованої "магії".
///
/// При невдалій валідації повертає 400 з ValidationProblemDetails
/// (RFC 7807) — той самий формат, що й глобальний exception middleware
/// (крок 9 плану), тому клієнт бачить консистентну структуру помилок
/// незалежно від того, валідація це чи виняток.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue; // для цього типу немає зареєстрованого валідатора — пропускаємо
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                var problemDetails = new ValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Одна або декілька помилок валідації."
                };

                context.Result = new BadRequestObjectResult(problemDetails);
                return;
            }
        }

        await next();
    }
}

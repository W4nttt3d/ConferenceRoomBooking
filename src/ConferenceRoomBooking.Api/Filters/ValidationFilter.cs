using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ConferenceRoomBooking.Api.Filters;

/// <summary>
/// Валідує вхідні DTO контролерів через FluentValidation, якщо для типу
/// аргументу зареєстровано IValidator&lt;T&gt;. Фільтр написаний вручну
/// замість пакета FluentValidation.AspNetCore - автоматична MVC-інтеграція
/// звідти визнана застарілою самим автором бібліотеки, а так поведінка
/// лишається явною і без прихованої "магії".
///
/// При невдалій валідації повертає 400 з ValidationProblemDetails - той
/// самий формат, що й глобальний exception handler для інших помилок.
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
                continue; // для цього типу немає зареєстрованого валідатора - пропускаємо
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
using ConferenceRoomBooking.Application.DTOs.Services;
using FluentValidation;

namespace ConferenceRoomBooking.Application.Validators.Services;

public class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequest>
{
    public CreateServiceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва послуги обов'язкова.")
            .MaximumLength(100).WithMessage("Назва послуги не може перевищувати 100 символів.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Ціна послуги має бути більшою за 0.");
    }
}

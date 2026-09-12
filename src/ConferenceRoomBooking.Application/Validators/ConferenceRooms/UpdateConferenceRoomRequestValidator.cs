using ConferenceRoomBooking.Application.DTOs.ConferenceRooms;
using FluentValidation;

namespace ConferenceRoomBooking.Application.Validators.ConferenceRooms;

public class UpdateConferenceRoomRequestValidator : AbstractValidator<UpdateConferenceRoomRequest>
{
    public UpdateConferenceRoomRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва залу обов'язкова.")
            .MaximumLength(100).WithMessage("Назва залу не може перевищувати 100 символів.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Місткість залу має бути більшою за 0.");

        RuleFor(x => x.BaseHourlyRate)
            .GreaterThan(0).WithMessage("Погодинна ставка має бути більшою за 0.");
    }
}

using ConferenceRoomBooking.Application.DTOs.ConferenceRooms;
using FluentValidation;

namespace ConferenceRoomBooking.Application.Validators.ConferenceRooms;

public class AvailableRoomsQueryValidator : AbstractValidator<AvailableRoomsQuery>
{
    public AvailableRoomsQueryValidator()
    {
        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Місткість має бути більшою за 0.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("Час завершення має бути пізніше за час початку.");
    }
}
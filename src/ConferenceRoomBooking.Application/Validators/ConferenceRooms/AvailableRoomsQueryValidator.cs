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

        // Свідоме рішення: тут НЕ перевіряються робочі години (06:00–23:00),
        // на відміну від CreateBookingRequestValidator. Пошук — це лише
        // читання, обмеження на робочі години застосовується один раз,
        // у єдиному місці — при фактичному створенні бронювання.
    }
}

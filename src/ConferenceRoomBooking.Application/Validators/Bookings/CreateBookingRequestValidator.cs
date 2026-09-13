using ConferenceRoomBooking.Application.DTOs.Bookings;
using FluentValidation;

namespace ConferenceRoomBooking.Application.Validators.Bookings;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    // Межі включні: 06:00 і 23:00 самі по собі дозволені.
    private static readonly TimeSpan WorkingHoursStart = new(6, 0, 0);
    private static readonly TimeSpan WorkingHoursEnd = new(23, 0, 0);

    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.ConferenceRoomId)
            .GreaterThan(0).WithMessage("Не вказано зал для бронювання.");

        RuleFor(x => x.StartTime)
            .GreaterThanOrEqualTo(_ => DateTime.Now)
            .WithMessage("Час початку бронювання не може бути в минулому.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("Час завершення має бути пізніше за час початку.");

        // Перевіряємо StartTime і EndTime окремо, щоб повідомлення про
        // помилку вказувало, яка саме межа порушена.
        RuleFor(x => x.StartTime)
            .Must(BeWithinWorkingHours)
            .WithMessage($"Час початку має бути в межах {WorkingHoursStart:hh\\:mm}-{WorkingHoursEnd:hh\\:mm}.");

        RuleFor(x => x.EndTime)
            .Must(BeWithinWorkingHours)
            .WithMessage($"Час завершення має бути в межах {WorkingHoursStart:hh\\:mm}-{WorkingHoursEnd:hh\\:mm}.");

        // Бронювання не може перетинати північ - вписується в робочі
        // години залу і спрощує погодинний розрахунок ціни.
        RuleFor(x => x)
            .Must(x => x.StartTime.Date == x.EndTime.Date)
            .WithMessage("Бронювання не може тривати довше одного календарного дня.")
            .OverridePropertyName(nameof(CreateBookingRequest.EndTime));

        RuleFor(x => x.ServiceIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Список послуг містить дублікати.");
    }

    private static bool BeWithinWorkingHours(DateTime dateTime)
    {
        return dateTime.TimeOfDay >= WorkingHoursStart && dateTime.TimeOfDay <= WorkingHoursEnd;
    }
}
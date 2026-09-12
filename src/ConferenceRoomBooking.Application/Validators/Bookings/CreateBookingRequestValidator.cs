using ConferenceRoomBooking.Application.DTOs.Bookings;
using FluentValidation;

namespace ConferenceRoomBooking.Application.Validators.Bookings;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    // Робочі години залів (див. Assumptions & Decisions у README):
    // межі включні — бронювання рівно з 06:00 або рівно до 23:00 дозволене.
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

        // Обидві межі перевіряються окремо, щоб повідомлення про помилку
        // чітко вказувало, яка саме межа (початок чи кінець) порушена.
        RuleFor(x => x.StartTime)
            .Must(BeWithinWorkingHours)
            .WithMessage($"Час початку має бути в межах {WorkingHoursStart:hh\\:mm}–{WorkingHoursEnd:hh\\:mm}.");

        RuleFor(x => x.EndTime)
            .Must(BeWithinWorkingHours)
            .WithMessage($"Час завершення має бути в межах {WorkingHoursStart:hh\\:mm}–{WorkingHoursEnd:hh\\:mm}.");

        // Рішення (задокументувати в README): бронювання не може перетинати
        // північ — це відповідає робочим годинам залу (06:00–23:00) і
        // спрощує погодинний розрахунок ціни (крок 10 плану).
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

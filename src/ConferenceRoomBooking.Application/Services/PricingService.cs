using ConferenceRoomBooking.Application.Interfaces;

namespace ConferenceRoomBooking.Application.Services;

/// <summary>
/// На відміну від ConferenceRoomService/ServiceService, цей сервіс не
/// потребує AppDbContext — це чиста функція розрахунку, тому він фізично
/// живе в Application (а не в Infrastructure), де й задекларований контракт.
/// </summary>
public class PricingService : IPricingService
{
    // Тарифні зони визначені як суміжні, невзаємоперетинні інтервали.
    // Рішення про пріоритет "Peak (12:00–14:00) вище за Standard" уже
    // враховане тут самими межами: Standard розбитий на 09:00–12:00 і
    // 14:00–18:00, а 12:00–14:00 виділений окремою зоною Peak.
    private static readonly (TimeSpan Start, TimeSpan End, decimal Multiplier)[] TariffZones =
    {
        (new TimeSpan(6, 0, 0), new TimeSpan(9, 0, 0), 0.90m),   // Morning:  -10%
        (new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0), 1.00m),  // Standard
        (new TimeSpan(12, 0, 0), new TimeSpan(14, 0, 0), 1.15m), // Peak:     +15%
        (new TimeSpan(14, 0, 0), new TimeSpan(18, 0, 0), 1.00m), // Standard
        (new TimeSpan(18, 0, 0), new TimeSpan(23, 0, 0), 0.80m)  // Evening:  -20%
    };

    public decimal CalculateRoomPrice(decimal baseHourlyRate, DateTime startTime, DateTime endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("EndTime має бути пізніше за StartTime.", nameof(endTime));
        }

        // Розбиваємо весь інтервал бронювання на сегменти по межах тарифних
        // зон, що потрапляють усередину — так бронювання, яке перетинає
        // кілька зон (наприклад, 11:00–15:00), рахується погодинно/по
        // сегментах, а не множенням середньої ставки на всю тривалість.
        var boundaries = new SortedSet<DateTime> { startTime, endTime };

        foreach (var zone in TariffZones)
        {
            AddBoundaryIfInside(boundaries, startTime.Date + zone.Start, startTime, endTime);
            AddBoundaryIfInside(boundaries, startTime.Date + zone.End, startTime, endTime);
        }

        var orderedBoundaries = boundaries.ToList();
        var total = 0m;

        for (var i = 0; i < orderedBoundaries.Count - 1; i++)
        {
            var segmentStart = orderedBoundaries[i];
            var segmentEnd = orderedBoundaries[i + 1];

            // Визначаємо тарифну зону по середині сегмента — так межова
            // точка (яка сама належить двом сусіднім сегментам) ніколи
            // не потрапляє в розрахунок як "середина".
            var midpoint = segmentStart + TimeSpan.FromTicks((segmentEnd - segmentStart).Ticks / 2);
            var multiplier = GetMultiplier(midpoint.TimeOfDay);
            var hours = (decimal)(segmentEnd - segmentStart).TotalHours;

            total += baseHourlyRate * hours * multiplier;
        }

        return decimal.Round(total, 2, MidpointRounding.AwayFromZero);
    }

    private static void AddBoundaryIfInside(ISet<DateTime> boundaries, DateTime candidate, DateTime start, DateTime end)
    {
        if (candidate > start && candidate < end)
        {
            boundaries.Add(candidate);
        }
    }

    private static decimal GetMultiplier(TimeSpan timeOfDay)
    {
        foreach (var zone in TariffZones)
        {
            if (timeOfDay >= zone.Start && timeOfDay < zone.End)
            {
                return zone.Multiplier;
            }
        }

        // Час поза визначеними тарифними зонами (< 06:00 або >= 23:00).
        // На практиці не повинно траплятися — CreateBookingRequestValidator
        // (крок 6) вже гарантує 06:00 <= booking <= 23:00. Базовий коефіцієнт
        // тут — лише запобіжник, а не сигнал про валідну ситуацію.
        return 1.00m;
    }
}

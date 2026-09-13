using ConferenceRoomBooking.Application.Interfaces;

namespace ConferenceRoomBooking.Application.Services;

/// <summary>
/// Чиста функція розрахунку без звернень до БД, тому живе в Application,
/// а не в Infrastructure (на відміну від ConferenceRoomService/ServiceService).
/// </summary>
public class PricingService : IPricingService
{
    // Зони суміжні й не перетинаються. Peak має пріоритет над Standard просто
    // тому, що Standard розбитий на дві частини (09:00-12:00 і 14:00-18:00),
    // а 12:00-14:00 виділений в окрему зону.
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

        // Розбиваємо інтервал на сегменти по межах тарифних зон і рахуємо
        // кожен окремо - бронювання, що перетинає кілька зон (наприклад,
        // 11:00-15:00), не можна порахувати одним множенням.
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

            // Зону беремо по середині сегмента, щоб межова точка (яка
            // належить одразу двом сусіднім сегментам) не збивала розрахунок.
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

        // Час поза межами 06:00-23:00 сюди дійти не повинен - валідатор
        // бронювання це відсікає раніше. Базовий коефіцієнт тут - просто
        // запобіжник, а не очікувана ситуація.
        return 1.00m;
    }
}
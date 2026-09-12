using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Domain.Enums;

namespace ConferenceRoomBooking.Domain.Specifications;

/// <summary>
/// Визначення "конфлікту бронювань" в одному місці — перевикористовується
/// і в пошуку доступних залів (крок 9), і в створенні бронювання (крок 11),
/// а також покривається окремими unit-тестами (крок 12+ плану).
/// </summary>
public static class BookingQueryExtensions
{
    /// <summary>
    /// Фільтрує бронювання, що конфліктують із заданим інтервалом
    /// [startTime, endTime).
    ///
    /// Правила:
    /// - Скасовані бронювання (Status == Cancelled) в конфлікт не враховуються.
    /// - Межі НЕ конфліктують: якщо одне бронювання закінчується рівно
    ///   тоді, коли починається інше (10:00–14:00 і 14:00–16:00), це OK.
    /// </summary>
    public static IQueryable<Booking> Overlapping(this IQueryable<Booking> bookings, DateTime startTime, DateTime endTime)
    {
        return bookings.Where(b =>
            b.Status != BookingStatus.Cancelled &&
            b.StartTime < endTime &&
            b.EndTime > startTime);
    }
}

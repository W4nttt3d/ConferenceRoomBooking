using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Domain.Enums;
using ConferenceRoomBooking.Domain.Specifications;
using Xunit;

namespace ConferenceRoomBooking.Tests.Domain.Specifications;

/// <summary>
/// Тести над чистою LINQ-логікою (Overlapping), без звернення до БД —
/// саме тут перевіряється найважливіше рішення проєкту: межова поведінка
/// конфлікту бронювань.
/// </summary>
public class BookingQueryExtensionsTests
{
    [Fact]
    public void Overlapping_ShouldDetectConflict_WhenIntervalsOverlap()
    {
        var bookings = new List<Booking>
        {
            new() { ConferenceRoomId = 1, StartTime = At(10, 0), EndTime = At(14, 0), Status = BookingStatus.Confirmed }
        };

        // Нове бронювання 12:00–15:00 частково перетинає існуюче 10:00–14:00.
        var result = bookings.AsQueryable().Overlapping(At(12, 0), At(15, 0)).ToList();

        Assert.Single(result);
    }

    [Fact]
    public void Overlapping_ExactBoundary_ShouldNotConflict()
    {
        var bookings = new List<Booking>
        {
            new() { ConferenceRoomId = 1, StartTime = At(10, 0), EndTime = At(14, 0), Status = BookingStatus.Confirmed }
        };

        // Нове бронювання починається рівно тоді, коли закінчується
        // попереднє (10:00–14:00 і 14:00–16:00) — це НЕ конфлікт.
        var result = bookings.AsQueryable().Overlapping(At(14, 0), At(16, 0)).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Overlapping_ShouldIgnoreCancelledBookings()
    {
        var bookings = new List<Booking>
        {
            new() { ConferenceRoomId = 1, StartTime = At(10, 0), EndTime = At(14, 0), Status = BookingStatus.Cancelled }
        };

        // Той самий інтервал, що й перше скасоване бронювання — конфлікту
        // бути не повинно, бо скасовані бронювання не враховуються.
        var result = bookings.AsQueryable().Overlapping(At(11, 0), At(13, 0)).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Overlapping_ShouldReturnEmpty_WhenNoBookingsExist()
    {
        var bookings = new List<Booking>();

        var result = bookings.AsQueryable().Overlapping(At(10, 0), At(11, 0)).ToList();

        Assert.Empty(result);
    }

    private static DateTime At(int hour, int minute) => new(2026, 9, 15, hour, minute, 0);
}

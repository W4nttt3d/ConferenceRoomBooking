using ConferenceRoomBooking.Domain.Enums;

namespace ConferenceRoomBooking.Domain.Entities;

/// <summary>
/// Бронювання конференц-залу на конкретний часовий інтервал.
/// </summary>
public class Booking
{
    public int Id { get; set; }

    public int ConferenceRoomId { get; set; }
    public ConferenceRoom ConferenceRoom { get; set; } = null!;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Підсумкова вартість: погодинна ставка залу з урахуванням тарифних
    /// зон (див. PricingService) + вартість обраних послуг.
    /// Розраховується один раз при створенні бронювання і не перераховується
    /// заднім числом, якщо зміняться тарифи чи ціни послуг.
    /// </summary>
    public decimal TotalPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
}

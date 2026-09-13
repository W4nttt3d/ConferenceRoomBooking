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

    /// <summary>
    /// Заповнюється базою через HasDefaultValueSql("now()") в
    /// BookingConfiguration. Свідомо без property initializer (= DateTime.UtcNow) -
    /// інакше EF Core відправив би це значення в INSERT замість DB-default,
    /// а Kind=Utc не сумісний з колонкою "timestamp without time zone".
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
}
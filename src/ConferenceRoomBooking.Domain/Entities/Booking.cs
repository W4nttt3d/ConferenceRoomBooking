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
    /// Заповнюється базою даних через HasDefaultValueSql("now()")
    /// (див. BookingConfiguration) — навмисно БЕЗ property initializer
    /// (= DateTime.UtcNow) тут. Такий initializer одразу проставляв би
    /// Kind=Utc ще в C#, і EF Core надсилав би це значення в INSERT,
    /// перекриваючи DB-default — а колонка "timestamp without time zone"
    /// (Assumptions & Decisions) вимагає саме Kind=Unspecified. Лишаючи
    /// властивість зі значенням CLR за замовчуванням (не встановленим),
    /// EF Core сам пропускає її в INSERT і дає БД згенерувати значення.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
}

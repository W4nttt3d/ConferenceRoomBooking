namespace ConferenceRoomBooking.Application.DTOs.Bookings;

/// <summary>
/// Дані для бронювання залу. Вартість розраховується сервером за тарифними
/// зонами (Morning/Standard/Peak/Evening) і не приймається від клієнта.
/// </summary>
public class CreateBookingRequest
{
    /// <summary>Id залу, який потрібно забронювати.</summary>
    /// <example>1</example>
    public int ConferenceRoomId { get; set; }

    /// <summary>
    /// Початок бронювання. Має бути не в минулому і в межах робочих годин
    /// залу (06:00–23:00).
    /// </summary>
    /// <example>2026-09-15T10:00:00</example>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Кінець бронювання. Має бути пізніше StartTime і в межах того самого
    /// календарного дня (бронювання не перетинає північ).
    /// </summary>
    /// <example>2026-09-15T14:00:00</example>
    public DateTime EndTime { get; set; }

    /// <summary>Id послуг, які додаються до бронювання. Може бути порожнім списком.</summary>
    /// <example>[1, 2]</example>
    public List<int> ServiceIds { get; set; } = new();
}

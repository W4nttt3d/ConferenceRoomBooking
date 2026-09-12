namespace ConferenceRoomBooking.Domain.Entities;

/// <summary>
/// Конференц-зал, доступний для бронювання.
/// </summary>
public class ConferenceRoom
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Максимальна кількість людей, яку вміщує зал.</summary>
    public int Capacity { get; set; }

    /// <summary>Базова погодинна ставка (без урахування тарифних коефіцієнтів і послуг).</summary>
    public decimal BaseHourlyRate { get; set; }

    /// <summary>
    /// Soft-delete прапорець замість фізичного видалення запису.
    /// Якщо зал уже мав бронювання, видаляти його з БД не можна —
    /// це знищить історію (TotalPrice, звіти тощо).
    /// </summary>
    public bool IsActive { get; set; } = true;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

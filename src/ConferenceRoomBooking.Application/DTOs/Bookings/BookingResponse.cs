namespace ConferenceRoomBooking.Application.DTOs.Bookings;

/// <summary>Підтверджене бронювання залу з розрахованою вартістю оренди.</summary>
public class BookingResponse
{
    /// <summary>Унікальний ідентифікатор бронювання.</summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>Id заброньованого залу.</summary>
    /// <example>1</example>
    public int ConferenceRoomId { get; set; }

    /// <summary>Назва заброньованого залу (для зручності — без додаткового запиту).</summary>
    /// <example>Зал А</example>
    public string ConferenceRoomName { get; set; } = string.Empty;

    /// <summary>Початок бронювання.</summary>
    /// <example>2026-09-15T10:00:00</example>
    public DateTime StartTime { get; set; }

    /// <summary>Кінець бронювання.</summary>
    /// <example>2026-09-15T14:00:00</example>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Загальна вартість (грн): оренда залу за тарифними зонами
    /// (Morning −10% / Standard / Peak +15% / Evening −20%) + вартість
    /// обраних послуг (кожна — один раз, незалежно від тривалості).
    /// </summary>
    /// <example>9200</example>
    public decimal TotalPrice { get; set; }

    /// <summary>Статус бронювання: "Confirmed" або "Cancelled".</summary>
    /// <example>Confirmed</example>
    public string Status { get; set; } = string.Empty;

    /// <summary>Список послуг, включених до бронювання.</summary>
    public List<BookingServiceResponse> Services { get; set; } = new();
}

namespace ConferenceRoomBooking.Application.DTOs.Bookings;

/// <summary>Одна послуга, включена до бронювання.</summary>
public class BookingServiceResponse
{
    /// <summary>Id послуги.</summary>
    /// <example>1</example>
    public int ServiceId { get; set; }

    /// <summary>Назва послуги на момент бронювання.</summary>
    /// <example>Проєктор</example>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>Ціна, зафіксована на момент бронювання (не поточна ціна послуги).</summary>
    /// <example>500</example>
    public decimal Price { get; set; }
}

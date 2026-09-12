namespace ConferenceRoomBooking.Application.DTOs.Bookings;

public class BookingServiceResponse
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>Ціна, зафіксована на момент бронювання (не поточна ціна послуги).</summary>
    public decimal Price { get; set; }
}

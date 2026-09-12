namespace ConferenceRoomBooking.Application.DTOs.Bookings;

public class CreateBookingRequest
{
    public int ConferenceRoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    /// <summary>Id послуг, які додаються до бронювання. Може бути порожнім списком.</summary>
    public List<int> ServiceIds { get; set; } = new();
}

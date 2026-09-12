namespace ConferenceRoomBooking.Application.DTOs.Bookings;

public class BookingResponse
{
    public int Id { get; set; }
    public int ConferenceRoomId { get; set; }
    public string ConferenceRoomName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<BookingServiceResponse> Services { get; set; } = new();
}

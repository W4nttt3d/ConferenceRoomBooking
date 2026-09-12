namespace ConferenceRoomBooking.Application.DTOs.ConferenceRooms;

public class CreateConferenceRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal BaseHourlyRate { get; set; }
}

namespace ConferenceRoomBooking.Application.DTOs.ConferenceRooms;

public class AvailableRoomsQuery
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }
}

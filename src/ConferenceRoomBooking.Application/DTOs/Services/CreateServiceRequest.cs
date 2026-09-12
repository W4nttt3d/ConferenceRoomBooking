namespace ConferenceRoomBooking.Application.DTOs.Services;

public class CreateServiceRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

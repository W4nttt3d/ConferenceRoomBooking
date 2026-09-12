namespace ConferenceRoomBooking.Application.DTOs.ConferenceRooms;

/// <summary>
/// Використовується і для звичайного CRUD, і для результатів пошуку
/// доступних залів (GET /api/conference-rooms/available) — набір полів
/// однаковий, окремий AvailableRoomResponse не додає нічого нового.
/// </summary>
public class ConferenceRoomResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal BaseHourlyRate { get; set; }
    public bool IsActive { get; set; }
}

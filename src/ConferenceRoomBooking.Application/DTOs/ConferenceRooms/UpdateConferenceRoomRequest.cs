namespace ConferenceRoomBooking.Application.DTOs.ConferenceRooms;

/// <summary>Дані для оновлення існуючого конференц-залу (повна заміна редагованих полів).</summary>
public class UpdateConferenceRoomRequest
{
    /// <summary>Нова назва залу.</summary>
    /// <example>Зал А (оновлений)</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>Нова максимальна кількість осіб.</summary>
    /// <example>60</example>
    public int Capacity { get; set; }

    /// <summary>Нова базова вартість оренди за годину (грн).</summary>
    /// <example>2500</example>
    public decimal BaseHourlyRate { get; set; }
}

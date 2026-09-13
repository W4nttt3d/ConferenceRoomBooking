namespace ConferenceRoomBooking.Application.DTOs.ConferenceRooms;

/// <summary>Дані для створення нового конференц-залу.</summary>
public class CreateConferenceRoomRequest
{
    /// <summary>Назва залу.</summary>
    /// <example>Зал А</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>Максимальна кількість осіб.</summary>
    /// <example>50</example>
    public int Capacity { get; set; }

    /// <summary>Базова вартість оренди за годину (грн), до застосування тарифних знижок/націнок.</summary>
    /// <example>2000</example>
    public decimal BaseHourlyRate { get; set; }
}

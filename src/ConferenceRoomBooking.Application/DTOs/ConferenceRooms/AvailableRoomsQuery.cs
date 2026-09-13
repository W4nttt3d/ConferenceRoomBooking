namespace ConferenceRoomBooking.Application.DTOs.ConferenceRooms;

/// <summary>Параметри пошуку залів, вільних на заданий інтервал часу.</summary>
public class AvailableRoomsQuery
{
    /// <summary>Початок бажаного інтервалу бронювання.</summary>
    /// <example>2026-09-15T10:00:00</example>
    public DateTime StartTime { get; set; }

    /// <summary>Кінець бажаного інтервалу бронювання (не раніше за StartTime).</summary>
    /// <example>2026-09-15T14:00:00</example>
    public DateTime EndTime { get; set; }

    /// <summary>Мінімальна необхідна місткість залу (осіб).</summary>
    /// <example>50</example>
    public int Capacity { get; set; }
}

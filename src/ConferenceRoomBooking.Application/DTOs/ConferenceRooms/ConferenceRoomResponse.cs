namespace ConferenceRoomBooking.Application.DTOs.ConferenceRooms;

/// <summary>
/// Використовується і для звичайного CRUD, і для результатів пошуку
/// доступних залів (GET /api/conference-rooms/available) — набір полів
/// однаковий, окремий AvailableRoomResponse не додає нічого нового.
/// </summary>
public class ConferenceRoomResponse
{
    /// <summary>Унікальний ідентифікатор залу.</summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>Назва залу.</summary>
    /// <example>Зал А</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>Максимальна кількість осіб.</summary>
    /// <example>50</example>
    public int Capacity { get; set; }

    /// <summary>Базова вартість оренди за годину (грн), до застосування тарифних знижок/націнок.</summary>
    /// <example>2000</example>
    public decimal BaseHourlyRate { get; set; }

    /// <summary>Чи доступний зал для бронювання (false — "видалений" через soft delete).</summary>
    /// <example>true</example>
    public bool IsActive { get; set; }
}

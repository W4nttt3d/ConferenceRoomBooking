namespace ConferenceRoomBooking.Application.DTOs.Services;

/// <summary>Дані для оновлення існуючої послуги.</summary>
public class UpdateServiceRequest
{
    /// <summary>Нова назва послуги.</summary>
    /// <example>Проєктор 4K</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Нова вартість послуги (грн). Впливає лише на майбутні бронювання —
    /// вже створені зберігають ціну, зафіксовану на момент бронювання
    /// (BookingService.Price), тож заднім числом не перераховуються.
    /// </summary>
    /// <example>550</example>
    public decimal Price { get; set; }
}

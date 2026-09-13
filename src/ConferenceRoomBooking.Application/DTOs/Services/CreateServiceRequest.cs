namespace ConferenceRoomBooking.Application.DTOs.Services;

/// <summary>Дані для створення нової додаткової послуги (проєктор, Wi-Fi, звук тощо).</summary>
public class CreateServiceRequest
{
    /// <summary>Назва послуги.</summary>
    /// <example>Проєктор</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>Вартість послуги (грн), фіксується в бронюванні на момент його створення.</summary>
    /// <example>500</example>
    public decimal Price { get; set; }
}

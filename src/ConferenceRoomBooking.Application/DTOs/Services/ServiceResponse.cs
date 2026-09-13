namespace ConferenceRoomBooking.Application.DTOs.Services;

/// <summary>Додаткова послуга, доступна для замовлення разом із бронюванням залу.</summary>
public class ServiceResponse
{
    /// <summary>Унікальний ідентифікатор послуги.</summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>Назва послуги.</summary>
    /// <example>Проєктор</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>Поточна вартість послуги (грн).</summary>
    /// <example>500</example>
    public decimal Price { get; set; }

    /// <summary>Чи доступна послуга для нових бронювань (false — "видалена" через soft delete).</summary>
    /// <example>true</example>
    public bool IsActive { get; set; }
}

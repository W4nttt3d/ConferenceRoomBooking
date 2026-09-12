namespace ConferenceRoomBooking.Domain.Entities;

/// <summary>
/// Додаткова послуга, яку можна замовити разом із бронюванням залу
/// (проєктор, Wi-Fi, звук тощо).
/// </summary>
public class Service
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Поточна ціна послуги. Не використовується напряму в уже створених
    /// бронюваннях — там зберігається зафіксована ціна на момент бронювання
    /// (див. <see cref="BookingService.Price"/>).
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>Soft-delete прапорець замість фізичного видалення.</summary>
    public bool IsActive { get; set; } = true;

    public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
}

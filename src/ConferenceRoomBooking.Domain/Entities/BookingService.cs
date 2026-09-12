namespace ConferenceRoomBooking.Domain.Entities;

/// <summary>
/// Зв'язуюча сутність між <see cref="Booking"/> і <see cref="Service"/>
/// (одне бронювання може мати декілька послуг, одна послуга може
/// використовуватись у багатьох бронюваннях).
/// </summary>
public class BookingService
{
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    /// <summary>
    /// Ціна послуги, зафіксована на момент бронювання.
    /// Навмисне рішення: якщо ціна послуги зміниться в майбутньому
    /// (наприклад, Проєктор подорожчає з 500 до 700 грн), старі
    /// бронювання повинні зберегти свою первісну вартість (500 грн).
    /// Тому ціна копіюється сюди при створенні, а не читається
    /// щоразу з Service.Price.
    /// </summary>
    public decimal Price { get; set; }
}

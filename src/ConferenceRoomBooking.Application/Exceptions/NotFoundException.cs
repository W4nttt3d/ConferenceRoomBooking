namespace ConferenceRoomBooking.Application.Exceptions;

/// <summary>
/// Сутність, на яку посилається запит, не знайдена (або недоступна для
/// використання — наприклад, неактивний зал). Контролер мапить це на 404.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}

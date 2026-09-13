namespace ConferenceRoomBooking.Application.Exceptions;

/// <summary>
/// Запит синтаксично коректний, але посилається на щось, що зараз не може
/// бути використане (наприклад, деактивована послуга). Контролер мапить
/// це на 422 Unprocessable Entity.
/// </summary>
public class UnprocessableEntityException : Exception
{
    public UnprocessableEntityException(string message) : base(message)
    {
    }
}

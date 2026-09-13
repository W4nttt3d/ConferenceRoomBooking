namespace ConferenceRoomBooking.Application.Exceptions;

/// <summary>
/// Зал вже заброньований на інтервал, що перетинається із запитуваним.
/// Контролер мапить це на 409 Conflict.
/// </summary>
public class BookingConflictException : Exception
{
    public BookingConflictException(string message) : base(message)
    {
    }
}

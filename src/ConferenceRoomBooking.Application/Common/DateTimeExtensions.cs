namespace ConferenceRoomBooking.Application.Common;

public static class DateTimeExtensions
{
    /// <summary>
    /// Приводить DateTime до Kind=Unspecified незалежно від того, звідки
    /// воно прийшло: query string без "Z" вже парситься як Unspecified,
    /// а JSON body з "Z" (наприклад, з дефолтного Swagger date-time picker)
    /// парситься System.Text.Json як Kind=Utc — і ці два випадки інакше
    /// вимагали б різної обробки.
    ///
    /// Потрібно, бо колонки StartTime/EndTime у БД — "timestamp without
    /// time zone" (Assumptions & Decisions: сервіс односайтовий, один
    /// часовий пояс, реальна UTC-конвертація не потрібна). Npgsql 8+
    /// вимагає, щоб Kind точно відповідав типу колонки: Unspecified для
    /// "without time zone", інакше — ArgumentException.
    /// </summary>
    public static DateTime AsUnspecifiedKind(this DateTime dateTime) =>
        DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
}

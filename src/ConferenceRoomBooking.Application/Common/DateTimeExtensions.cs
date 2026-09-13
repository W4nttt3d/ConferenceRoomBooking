namespace ConferenceRoomBooking.Application.Common;

public static class DateTimeExtensions
{
    /// <summary>
    /// Приводить DateTime до Kind=Unspecified незалежно від джерела: query
    /// string без "Z" вже парситься як Unspecified, а JSON body з "Z"
    /// (наприклад, зі Swagger date-time picker) - як Kind=Utc.
    ///
    /// Потрібно, бо StartTime/EndTime зберігаються як "timestamp without
    /// time zone" - Npgsql 8+ вимагає точної відповідності Kind і типу
    /// колонки, інакше кидає ArgumentException.
    /// </summary>
    public static DateTime AsUnspecifiedKind(this DateTime dateTime) =>
        DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
}
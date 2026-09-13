namespace ConferenceRoomBooking.Application.Interfaces;

public interface IPricingService
{
    /// <summary>
    /// Розраховує вартість оренди залу за інтервал [startTime, endTime)
    /// з урахуванням тарифних коефіцієнтів (06:00-09:00 -10%, 12:00-14:00
    /// +15%, 18:00-23:00 -20%, решта - базова ставка). Не включає вартість
    /// додаткових послуг - ті додаються окремо (BookingService.CreateAsync).
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Якщо endTime не пізніше за startTime.
    /// </exception>
    decimal CalculateRoomPrice(decimal baseHourlyRate, DateTime startTime, DateTime endTime);
}
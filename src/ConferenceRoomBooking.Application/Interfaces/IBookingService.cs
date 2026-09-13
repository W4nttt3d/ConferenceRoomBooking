using ConferenceRoomBooking.Application.DTOs.Bookings;

namespace ConferenceRoomBooking.Application.Interfaces;

public interface IBookingService
{
    Task<IReadOnlyList<BookingResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Повертає null, якщо бронювання з таким Id не знайдено.</summary>
    Task<BookingResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Створює бронювання. Кидає:
    /// - NotFoundException — якщо зал/послуга не знайдені, або зал неактивний;
    /// - BookingConflictException — якщо зал вже заброньований на цей інтервал;
    /// - UnprocessableEntityException — якщо обрана послуга деактивована.
    /// </summary>
    Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);
}

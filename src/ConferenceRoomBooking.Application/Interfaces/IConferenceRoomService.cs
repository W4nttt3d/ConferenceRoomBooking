using ConferenceRoomBooking.Application.DTOs.ConferenceRooms;

namespace ConferenceRoomBooking.Application.Interfaces;

public interface IConferenceRoomService
{
    Task<IReadOnlyList<ConferenceRoomResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Повертає null, якщо зал з таким Id не знайдено.</summary>
    Task<ConferenceRoomResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ConferenceRoomResponse> CreateAsync(CreateConferenceRoomRequest request, CancellationToken cancellationToken = default);

    /// <summary>Повертає null, якщо зал з таким Id не знайдено.</summary>
    Task<ConferenceRoomResponse?> UpdateAsync(int id, UpdateConferenceRoomRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete: виставляє IsActive = false замість фізичного видалення запису.
    /// Повертає false, якщо зал з таким Id не знайдено.
    /// </summary>
    Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default);
}

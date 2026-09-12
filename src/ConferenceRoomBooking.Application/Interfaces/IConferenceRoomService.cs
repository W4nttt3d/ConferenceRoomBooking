using ConferenceRoomBooking.Application.DTOs.ConferenceRooms;

namespace ConferenceRoomBooking.Application.Interfaces;

public interface IConferenceRoomService
{
    Task<IReadOnlyList<ConferenceRoomResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Пошук активних залів із достатньою місткістю, вільних на заданий
    /// інтервал часу (без конфліктуючих бронювань).
    /// </summary>
    Task<IReadOnlyList<ConferenceRoomResponse>> SearchAvailableAsync(
        DateTime startTime,
        DateTime endTime,
        int capacity,
        CancellationToken cancellationToken = default);

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

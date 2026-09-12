using ConferenceRoomBooking.Application.DTOs.Services;

namespace ConferenceRoomBooking.Application.Interfaces;

/// <summary>
/// CRUD для додаткових послуг (проєктор, Wi-Fi, звук тощо).
/// Назва трохи незграбна (Service...Service), але узгоджена з тим, як
/// названо саму сутність Domain.Entities.Service.
/// </summary>
public interface IServiceService
{
    Task<IReadOnlyList<ServiceResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Повертає null, якщо послугу з таким Id не знайдено.</summary>
    Task<ServiceResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ServiceResponse> CreateAsync(CreateServiceRequest request, CancellationToken cancellationToken = default);

    /// <summary>Повертає null, якщо послугу з таким Id не знайдено.</summary>
    Task<ServiceResponse?> UpdateAsync(int id, UpdateServiceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete: виставляє IsActive = false. Повертає false, якщо
    /// послугу з таким Id не знайдено.
    /// </summary>
    Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default);
}

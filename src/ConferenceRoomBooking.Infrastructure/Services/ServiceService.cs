using ConferenceRoomBooking.Application.DTOs.Services;
using ConferenceRoomBooking.Application.Interfaces;
using ConferenceRoomBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using DomainService = ConferenceRoomBooking.Domain.Entities.Service;

namespace ConferenceRoomBooking.Infrastructure.Services;

public class ServiceService : IServiceService
{
    private readonly AppDbContext _context;

    public ServiceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ServiceResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Services
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => ToResponse(s))
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var service = await _context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        return service is null ? null : ToResponse(service);
    }

    public async Task<ServiceResponse> CreateAsync(CreateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var service = new DomainService
        {
            Name = request.Name,
            Price = request.Price,
            IsActive = true
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(service);
    }

    public async Task<ServiceResponse?> UpdateAsync(int id, UpdateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (service is null)
        {
            return null;
        }

        // Навмисне рішення: змінюємо Price напряму на сутності Service.
        // Уже створені бронювання не постраждають — там зберігається
        // BookingService.Price, зафіксований окремо на момент бронювання.
        service.Name = request.Name;
        service.Price = request.Price;

        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(service);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (service is null)
        {
            return false;
        }

        service.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static ServiceResponse ToResponse(DomainService service) => new()
    {
        Id = service.Id,
        Name = service.Name,
        Price = service.Price,
        IsActive = service.IsActive
    };
}

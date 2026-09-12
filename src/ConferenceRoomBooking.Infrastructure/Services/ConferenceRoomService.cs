using ConferenceRoomBooking.Application.DTOs.ConferenceRooms;
using ConferenceRoomBooking.Application.Interfaces;
using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBooking.Infrastructure.Services;

public class ConferenceRoomService : IConferenceRoomService
{
    private readonly AppDbContext _context;

    public ConferenceRoomService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ConferenceRoomResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ConferenceRooms
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => ToResponse(r))
            .ToListAsync(cancellationToken);
    }

    public async Task<ConferenceRoomResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var room = await _context.ConferenceRooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        return room is null ? null : ToResponse(room);
    }

    public async Task<ConferenceRoomResponse> CreateAsync(CreateConferenceRoomRequest request, CancellationToken cancellationToken = default)
    {
        var room = new ConferenceRoom
        {
            Name = request.Name,
            Capacity = request.Capacity,
            BaseHourlyRate = request.BaseHourlyRate,
            IsActive = true
        };

        _context.ConferenceRooms.Add(room);
        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(room);
    }

    public async Task<ConferenceRoomResponse?> UpdateAsync(int id, UpdateConferenceRoomRequest request, CancellationToken cancellationToken = default)
    {
        var room = await _context.ConferenceRooms
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (room is null)
        {
            return null;
        }

        room.Name = request.Name;
        room.Capacity = request.Capacity;
        room.BaseHourlyRate = request.BaseHourlyRate;

        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(room);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var room = await _context.ConferenceRooms
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (room is null)
        {
            return false;
        }

        // Soft delete — ніколи не видаляємо зал фізично, щоб не втратити
        // історію бронювань, які на нього посилаються.
        room.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static ConferenceRoomResponse ToResponse(ConferenceRoom room) => new()
    {
        Id = room.Id,
        Name = room.Name,
        Capacity = room.Capacity,
        BaseHourlyRate = room.BaseHourlyRate,
        IsActive = room.IsActive
    };
}

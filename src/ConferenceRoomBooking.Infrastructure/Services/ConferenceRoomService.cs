using ConferenceRoomBooking.Application.Common;
using ConferenceRoomBooking.Application.DTOs.ConferenceRooms;
using ConferenceRoomBooking.Application.Interfaces;
using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Domain.Specifications;
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

    public async Task<IReadOnlyList<ConferenceRoomResponse>> SearchAvailableAsync(
        DateTime startTime,
        DateTime endTime,
        int capacity,
        CancellationToken cancellationToken = default)
    {
        // Kind нормалізуємо так само, як у BookingService.CreateAsync -
        // колонки StartTime/EndTime в БД без часового поясу.
        startTime = startTime.AsUnspecifiedKind();
        endTime = endTime.AsUnspecifiedKind();

        // Спочатку окремим запитом дістаємо Id залів з конфліктом, а не
        // фільтруємо через вкладений Any() по навігації - так логіка
        // конфлікту лишається в одному місці (Overlapping) і легко тестується.
        var roomIdsWithConflict = await _context.Bookings
            .Overlapping(startTime, endTime)
            .Select(b => b.ConferenceRoomId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context.ConferenceRooms
            .AsNoTracking()
            .Where(r => r.IsActive && r.Capacity >= capacity && !roomIdsWithConflict.Contains(r.Id))
            .OrderBy(r => r.Name)
            .Select(r => ToResponse(r))
            .ToListAsync(cancellationToken);
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

        // Soft delete - ніколи не видаляємо зал фізично, щоб не втратити
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
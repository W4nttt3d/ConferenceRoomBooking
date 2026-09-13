using System.Data;
using ConferenceRoomBooking.Application.Common;
using ConferenceRoomBooking.Application.DTOs.Bookings;
using ConferenceRoomBooking.Application.Exceptions;
using ConferenceRoomBooking.Application.Interfaces;
using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Domain.Specifications;
using ConferenceRoomBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using DomainService = ConferenceRoomBooking.Domain.Entities.Service;
using DomainBookingService = ConferenceRoomBooking.Domain.Entities.BookingService;

namespace ConferenceRoomBooking.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly IPricingService _pricingService;

    public BookingService(AppDbContext context, IPricingService pricingService)
    {
        _context = context;
        _pricingService = pricingService;
    }

    public async Task<IReadOnlyList<BookingResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var bookings = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.ConferenceRoom)
            .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.Service)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return bookings.Select(ToResponse).ToList();
    }

    public async Task<BookingResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.ConferenceRoom)
            .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.Service)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        return booking is null ? null : ToResponse(booking);
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        // Нормалізація Kind — див. DateTimeExtensions.AsUnspecifiedKind().
        // Робимо це один раз, на самому вході в метод, і далі скрізь
        // використовуємо ці локальні змінні замість request.StartTime/EndTime.
        var startTime = request.StartTime.AsUnspecifiedKind();
        var endTime = request.EndTime.AsUnspecifiedKind();

        // 1-2. Зал існує і активний.
        var room = await _context.ConferenceRooms
            .FirstOrDefaultAsync(r => r.Id == request.ConferenceRoomId, cancellationToken)
            ?? throw new NotFoundException($"Зал з Id = {request.ConferenceRoomId} не знайдено.");

        if (!room.IsActive)
        {
            throw new NotFoundException($"Зал з Id = {request.ConferenceRoomId} неактивний і недоступний для бронювання.");
        }

        // 3. Правильність часу (StartTime < EndTime, робочі години) вже
        // перевірена CreateBookingRequestValidator через ValidationFilter
        // до того, як виконання взагалі дійшло до цього сервісу.

        // 5-6. Послуги існують і активні.
        var requestedServiceIds = request.ServiceIds.Distinct().ToList();
        var services = requestedServiceIds.Count == 0
            ? new List<DomainService>()
            : await _context.Services
                .Where(s => requestedServiceIds.Contains(s.Id))
                .ToListAsync(cancellationToken);

        var missingServiceIds = requestedServiceIds.Except(services.Select(s => s.Id)).ToList();
        if (missingServiceIds.Count > 0)
        {
            throw new NotFoundException(
                $"Послуги з Id = [{string.Join(", ", missingServiceIds)}] не знайдено.");
        }

        var inactiveServices = services.Where(s => !s.IsActive).ToList();
        if (inactiveServices.Count > 0)
        {
            throw new UnprocessableEntityException(
                $"Послуги [{string.Join(", ", inactiveServices.Select(s => s.Name))}] " +
                "наразі недоступні для бронювання.");
        }

        // 7. Розрахунок вартості: оренда залу (PricingService, крок 10)
        // + послуги (додаються один раз, без множення на тривалість).
        var roomPrice = _pricingService.CalculateRoomPrice(room.BaseHourlyRate, startTime, endTime);
        var servicesPrice = services.Sum(s => s.Price);

        var booking = new Booking
        {
            ConferenceRoomId = room.Id,
            StartTime = startTime,
            EndTime = endTime,
            TotalPrice = roomPrice + servicesPrice,
            BookingServices = services.Select(s => new DomainBookingService
            {
                ServiceId = s.Id,
                Price = s.Price
            }).ToList()
        };

        // 4 + 8-9. Перевірка конфлікту і створення бронювання — в ОДНІЙ
        // транзакції з IsolationLevel.Serializable. Без цього два одночасні
        // запити на той самий інтервал теоретично можуть обидва пройти
        // перевірку конфлікту (обидва бачать "вільно") і створити накладені
        // бронювання. Serializable гарантує: якщо два такі запити
        // виконуються одночасно, PostgreSQL сам відкотить один з них з
        // помилкою serialization_failure, навіть якщо обидві транзакції
        // локально "не побачили" конфлікту одна одної.
        //
        // Транзакція вмикається лише для реляційного провайдера — EF Core
        // InMemory (використовується в unit-тестах) не підтримує
        // BeginTransactionAsync(IsolationLevel), а сам є однопотоковим,
        // тому race condition там у принципі неможливий.
        var isRelational = _context.Database.IsRelational();
        IDbContextTransaction? transaction = isRelational
            ? await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            : null;

        try
        {
            var hasConflict = await _context.Bookings
                .Where(b => b.ConferenceRoomId == request.ConferenceRoomId)
                .Overlapping(startTime, endTime)
                .AnyAsync(cancellationToken);

            if (hasConflict)
            {
                throw new BookingConflictException(
                    $"Зал з Id = {request.ConferenceRoomId} вже заброньований на період " +
                    $"{startTime:HH:mm}–{endTime:HH:mm}.");
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync(cancellationToken);

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.SerializationFailure)
        {
            // PostgreSQL сам виявив конфлікт серіалізації між двома
            // одночасними Serializable-транзакціями — навіть якщо наша
            // перевірка вище (AnyAsync) цього не побачила через timing.
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            throw new BookingConflictException(
                $"Зал з Id = {request.ConferenceRoomId} вже заброньований на цей інтервал " +
                "(виявлено конфлікт одночасного доступу).");
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            throw;
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }

        // Підвантажуємо ConferenceRoom/Service для мапінгу у відповідь
        // (уникаємо додаткового round-trip: room уже в пам'яті, а
        // Service-об'єкти беремо з уже завантаженого списку services).
        return new BookingResponse
        {
            Id = booking.Id,
            ConferenceRoomId = room.Id,
            ConferenceRoomName = room.Name,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status.ToString(),
            Services = services.Select(s => new BookingServiceResponse
            {
                ServiceId = s.Id,
                ServiceName = s.Name,
                Price = s.Price
            }).ToList()
        };
    }

    private static BookingResponse ToResponse(Booking booking) => new()
    {
        Id = booking.Id,
        ConferenceRoomId = booking.ConferenceRoomId,
        ConferenceRoomName = booking.ConferenceRoom.Name,
        StartTime = booking.StartTime,
        EndTime = booking.EndTime,
        TotalPrice = booking.TotalPrice,
        Status = booking.Status.ToString(),
        Services = booking.BookingServices.Select(bs => new BookingServiceResponse
        {
            ServiceId = bs.ServiceId,
            ServiceName = bs.Service.Name,
            Price = bs.Price
        }).ToList()
    };
}

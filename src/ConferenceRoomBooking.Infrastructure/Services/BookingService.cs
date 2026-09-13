using ConferenceRoomBooking.Application.DTOs.Bookings;
using ConferenceRoomBooking.Application.Exceptions;
using ConferenceRoomBooking.Application.Interfaces;
using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Domain.Specifications;
using ConferenceRoomBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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
        // 1-2. Зал існує і активний.
        // ПРИМІТКА (крок 12 плану — concurrency): весь метод CreateAsync,
        // від перевірки конфлікту до SaveChangesAsync, буде обгорнутий в
        // транзакцію з IsolationLevel.Serializable. Наразі без цього два
        // одночасні запити теоретично можуть обидва пройти перевірку
        // конфлікту й створити накладені бронювання — свідомо залишено
        // на наступний крок, щоб не змішувати два різні за складністю
        // завдання в одному коміті.
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

        // 4. Конфлікт бронювання — той самий Overlapping(), що й у пошуку
        // доступних залів (крок 9), тепер звужений до конкретного залу.
        var hasConflict = await _context.Bookings
            .Where(b => b.ConferenceRoomId == request.ConferenceRoomId)
            .Overlapping(request.StartTime, request.EndTime)
            .AnyAsync(cancellationToken);

        if (hasConflict)
        {
            throw new BookingConflictException(
                $"Зал з Id = {request.ConferenceRoomId} вже заброньований на період " +
                $"{request.StartTime:HH:mm}–{request.EndTime:HH:mm}.");
        }

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
        var roomPrice = _pricingService.CalculateRoomPrice(room.BaseHourlyRate, request.StartTime, request.EndTime);
        var servicesPrice = services.Sum(s => s.Price);

        // 8-9. Створення Booking + BookingService (ціна послуги
        // фіксується тут, у BookingService.Price — див. коментар
        // у Domain.Entities.BookingService).
        var booking = new Booking
        {
            ConferenceRoomId = room.Id,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            TotalPrice = roomPrice + servicesPrice,
            BookingServices = services.Select(s => new DomainBookingService
            {
                ServiceId = s.Id,
                Price = s.Price
            }).ToList()
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken);

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

using ConferenceRoomBooking.Application.DTOs.Bookings;
using ConferenceRoomBooking.Application.Exceptions;
using ConferenceRoomBooking.Application.Services;
using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Tests.Common;
using Xunit;
using BookingServiceUnderTest = ConferenceRoomBooking.Infrastructure.Services.BookingService;

namespace ConferenceRoomBooking.Tests.Infrastructure.Services;

/// <summary>
/// PricingService тут використовується справжній (не мок) — він чиста
/// швидка функція без побічних ефектів, тому мокати його не має сенсу;
/// це заразом перевіряє й коректну інтеграцію двох сервісів.
/// </summary>
public class BookingServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldThrowNotFound_WhenRoomDoesNotExist()
    {
        var context = TestDbContextFactory.Create();
        var sut = new BookingServiceUnderTest(context, new PricingService());

        var request = new CreateBookingRequest
        {
            ConferenceRoomId = 999,
            StartTime = new DateTime(2026, 9, 15, 10, 0, 0),
            EndTime = new DateTime(2026, 9, 15, 11, 0, 0)
        };

        await Assert.ThrowsAsync<NotFoundException>(() => sut.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFound_WhenRoomIsInactive()
    {
        var context = TestDbContextFactory.Create();
        var room = new ConferenceRoom { Name = "Зал А", Capacity = 50, BaseHourlyRate = 2000m, IsActive = false };
        context.ConferenceRooms.Add(room);
        await context.SaveChangesAsync();

        var sut = new BookingServiceUnderTest(context, new PricingService());
        var request = new CreateBookingRequest
        {
            ConferenceRoomId = room.Id,
            StartTime = new DateTime(2026, 9, 15, 10, 0, 0),
            EndTime = new DateTime(2026, 9, 15, 11, 0, 0)
        };

        await Assert.ThrowsAsync<NotFoundException>(() => sut.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowConflict_WhenRoomAlreadyBooked()
    {
        var context = TestDbContextFactory.Create();
        var room = new ConferenceRoom { Name = "Зал А", Capacity = 50, BaseHourlyRate = 2000m, IsActive = true };
        context.ConferenceRooms.Add(room);
        await context.SaveChangesAsync();

        context.Bookings.Add(new Booking
        {
            ConferenceRoomId = room.Id,
            StartTime = new DateTime(2026, 9, 15, 10, 0, 0),
            EndTime = new DateTime(2026, 9, 15, 14, 0, 0)
        });
        await context.SaveChangesAsync();

        var sut = new BookingServiceUnderTest(context, new PricingService());
        var request = new CreateBookingRequest
        {
            ConferenceRoomId = room.Id,
            StartTime = new DateTime(2026, 9, 15, 12, 0, 0), // перетинає 10:00–14:00
            EndTime = new DateTime(2026, 9, 15, 15, 0, 0)
        };

        await Assert.ThrowsAsync<BookingConflictException>(() => sut.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenTimesDoNotOverlap()
    {
        var context = TestDbContextFactory.Create();
        var room = new ConferenceRoom { Name = "Зал А", Capacity = 50, BaseHourlyRate = 2000m, IsActive = true };
        context.ConferenceRooms.Add(room);
        await context.SaveChangesAsync();

        context.Bookings.Add(new Booking
        {
            ConferenceRoomId = room.Id,
            StartTime = new DateTime(2026, 9, 15, 10, 0, 0),
            EndTime = new DateTime(2026, 9, 15, 14, 0, 0)
        });
        await context.SaveChangesAsync();

        var sut = new BookingServiceUnderTest(context, new PricingService());
        var request = new CreateBookingRequest
        {
            ConferenceRoomId = room.Id,
            StartTime = new DateTime(2026, 9, 15, 14, 0, 0), // рівно з моменту завершення попереднього
            EndTime = new DateTime(2026, 9, 15, 16, 0, 0)
        };

        var result = await sut.CreateAsync(request);

        Assert.NotEqual(0, result.Id);
        Assert.Equal("Зал А", result.ConferenceRoomName);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFound_WhenServiceDoesNotExist()
    {
        var context = TestDbContextFactory.Create();
        var room = new ConferenceRoom { Name = "Зал А", Capacity = 50, BaseHourlyRate = 2000m, IsActive = true };
        context.ConferenceRooms.Add(room);
        await context.SaveChangesAsync();

        var sut = new BookingServiceUnderTest(context, new PricingService());
        var request = new CreateBookingRequest
        {
            ConferenceRoomId = room.Id,
            StartTime = new DateTime(2026, 9, 15, 10, 0, 0),
            EndTime = new DateTime(2026, 9, 15, 11, 0, 0),
            ServiceIds = new List<int> { 999 }
        };

        await Assert.ThrowsAsync<NotFoundException>(() => sut.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowUnprocessable_WhenServiceIsInactive()
    {
        var context = TestDbContextFactory.Create();
        var room = new ConferenceRoom { Name = "Зал А", Capacity = 50, BaseHourlyRate = 2000m, IsActive = true };
        var service = new Service { Name = "Проєктор", Price = 500m, IsActive = false };
        context.ConferenceRooms.Add(room);
        context.Services.Add(service);
        await context.SaveChangesAsync();

        var sut = new BookingServiceUnderTest(context, new PricingService());
        var request = new CreateBookingRequest
        {
            ConferenceRoomId = room.Id,
            StartTime = new DateTime(2026, 9, 15, 10, 0, 0),
            EndTime = new DateTime(2026, 9, 15, 11, 0, 0),
            ServiceIds = new List<int> { service.Id }
        };

        await Assert.ThrowsAsync<UnprocessableEntityException>(() => sut.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_ShouldIncludeServicesPriceInTotal()
    {
        var context = TestDbContextFactory.Create();
        var room = new ConferenceRoom { Name = "Зал А", Capacity = 50, BaseHourlyRate = 2000m, IsActive = true };
        var service = new Service { Name = "Проєктор", Price = 500m, IsActive = true };
        context.ConferenceRooms.Add(room);
        context.Services.Add(service);
        await context.SaveChangesAsync();

        var sut = new BookingServiceUnderTest(context, new PricingService());
        var request = new CreateBookingRequest
        {
            ConferenceRoomId = room.Id,
            StartTime = new DateTime(2026, 9, 15, 10, 0, 0), // Standard-зона, 1 година
            EndTime = new DateTime(2026, 9, 15, 11, 0, 0),
            ServiceIds = new List<int> { service.Id }
        };

        var result = await sut.CreateAsync(request);

        // 2000 (оренда, 1г × Standard 1.00) + 500 (послуга) = 2500
        Assert.Equal(2500m, result.TotalPrice);
        Assert.Single(result.Services);
        Assert.Equal(500m, result.Services[0].Price);
    }
}

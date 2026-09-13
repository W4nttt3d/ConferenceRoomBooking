using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Tests.Common;
using Xunit;

namespace ConferenceRoomBooking.Tests.Infrastructure.Services;

/// <summary>
/// Тут перевіряється правило "недостатня місткість" з плану
/// (CannotBookRoomWithInsufficientCapacity) — воно застосовується саме
/// при пошуку доступних залів, а не при створенні бронювання: якщо
/// бронювання створюється напряму за ConferenceRoomId, місткість уже
/// відома з моменту вибору залу і повторно не перевіряється.
/// </summary>
public class ConferenceRoomServiceAvailabilityTests
{
    [Fact]
    public async Task SearchAvailableAsync_ShouldExcludeRoomsWithInsufficientCapacity()
    {
        var context = TestDbContextFactory.Create();
        context.ConferenceRooms.AddRange(
            new ConferenceRoom { Name = "Малий зал", Capacity = 10, BaseHourlyRate = 1000m, IsActive = true },
            new ConferenceRoom { Name = "Великий зал", Capacity = 100, BaseHourlyRate = 3000m, IsActive = true });
        await context.SaveChangesAsync();

        var sut = new ConferenceRoomBooking.Infrastructure.Services.ConferenceRoomService(context);

        var result = await sut.SearchAvailableAsync(
            new DateTime(2026, 9, 15, 10, 0, 0),
            new DateTime(2026, 9, 15, 11, 0, 0),
            capacity: 50);

        Assert.Single(result);
        Assert.Equal("Великий зал", result[0].Name);
    }

    [Fact]
    public async Task SearchAvailableAsync_ShouldExcludeInactiveRooms()
    {
        var context = TestDbContextFactory.Create();
        context.ConferenceRooms.Add(
            new ConferenceRoom { Name = "Зал А", Capacity = 50, BaseHourlyRate = 2000m, IsActive = false });
        await context.SaveChangesAsync();

        var sut = new ConferenceRoomBooking.Infrastructure.Services.ConferenceRoomService(context);

        var result = await sut.SearchAvailableAsync(
            new DateTime(2026, 9, 15, 10, 0, 0),
            new DateTime(2026, 9, 15, 11, 0, 0),
            capacity: 1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAvailableAsync_ShouldExcludeRoomsWithConflictingBooking()
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

        var sut = new ConferenceRoomBooking.Infrastructure.Services.ConferenceRoomService(context);

        var result = await sut.SearchAvailableAsync(
            new DateTime(2026, 9, 15, 12, 0, 0), // перетинає 10:00–14:00
            new DateTime(2026, 9, 15, 15, 0, 0),
            capacity: 1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAvailableAsync_ShouldIncludeRoom_WhenBookingDoesNotOverlap()
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

        var sut = new ConferenceRoomBooking.Infrastructure.Services.ConferenceRoomService(context);

        // Рівно з моменту завершення існуючого бронювання — не конфлікт.
        var result = await sut.SearchAvailableAsync(
            new DateTime(2026, 9, 15, 14, 0, 0),
            new DateTime(2026, 9, 15, 16, 0, 0),
            capacity: 1);

        Assert.Single(result);
    }
}

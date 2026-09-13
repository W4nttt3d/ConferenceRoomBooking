using System.Net;
using System.Net.Http.Json;
using ConferenceRoomBooking.Application.DTOs.ConferenceRooms;
using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Domain.Enums;
using Xunit;

namespace ConferenceRoomBooking.Tests.Api.IntegrationTests;

/// <summary>
/// Integration-тести GET /api/conference-rooms/available. На відміну від
/// Infrastructure.Services.ConferenceRoomServiceAvailabilityTests (unit,
/// сервіс напряму), тут перевіряється весь ланцюжок: query-параметри →
/// AvailableRoomsQueryValidator → controller → серіалізація відповіді.
/// </summary>
public class AvailableRoomsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public AvailableRoomsTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAvailable_ShouldReturnOnlyActiveRoomsWithEnoughCapacityAndNoConflict()
    {
        var searchStart = DateTime.Today.AddDays(1).AddHours(10); // 10:00 завтра
        var searchEnd = searchStart.AddHours(4);                  // 14:00 завтра

        var roomWithEnoughCapacity = new ConferenceRoom
        { Name = "Зал Великий", Capacity = 100, BaseHourlyRate = 3000m, IsActive = true };
        var roomTooSmall = new ConferenceRoom
        { Name = "Зал Малий", Capacity = 10, BaseHourlyRate = 1000m, IsActive = true };
        var roomInactive = new ConferenceRoom
        { Name = "Зал Неактивний", Capacity = 100, BaseHourlyRate = 3000m, IsActive = false };

        await _factory.SeedAsync(ctx =>
        {
            ctx.ConferenceRooms.AddRange(roomWithEnoughCapacity, roomTooSmall, roomInactive);
        });

        var response = await _client.GetAsync(
            $"/api/conference-rooms/available?startTime={searchStart:s}&endTime={searchEnd:s}&capacity=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var rooms = await response.Content.ReadFromJsonAsync<List<ConferenceRoomResponse>>();

        Assert.NotNull(rooms);
        // Малий (замала місткість) і неактивний зали відсіяні — лишається тільки Великий.
        var room = Assert.Single(rooms!);
        Assert.Equal(roomWithEnoughCapacity.Name, room.Name);
    }

    [Fact]
    public async Task GetAvailable_ShouldExcludeRoomWithConflictingBooking()
    {
        var searchStart = DateTime.Today.AddDays(1).AddHours(10);
        var searchEnd = searchStart.AddHours(4); // 10:00–14:00 завтра

        var room = new ConferenceRoom
        { Name = "Зал А", Capacity = 50, BaseHourlyRate = 2000m, IsActive = true };

        await _factory.SeedAsync(ctx =>
        {
            ctx.ConferenceRooms.Add(room);
            // Існуюче бронювання 11:00–12:00 перетинається із запитуваним
            // інтервалом 10:00–14:00 — зал має бути виключений з результату.
            ctx.Bookings.Add(new Booking
            {
                ConferenceRoomId = room.Id,
                ConferenceRoom = room,
                StartTime = searchStart.AddHours(1),
                EndTime = searchStart.AddHours(2),
                TotalPrice = 2000m,
                Status = BookingStatus.Confirmed
            });
        });

        var response = await _client.GetAsync(
            $"/api/conference-rooms/available?startTime={searchStart:s}&endTime={searchEnd:s}&capacity=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var rooms = await response.Content.ReadFromJsonAsync<List<ConferenceRoomResponse>>();

        Assert.NotNull(rooms);
        Assert.Empty(rooms!);
    }

    [Fact]
    public async Task GetAvailable_WithEndTimeBeforeStartTime_ShouldReturn400()
    {
        var start = DateTime.Today.AddDays(1).AddHours(14);
        var end = DateTime.Today.AddDays(1).AddHours(10); // раніше за start

        var response = await _client.GetAsync(
            $"/api/conference-rooms/available?startTime={start:s}&endTime={end:s}&capacity=10");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
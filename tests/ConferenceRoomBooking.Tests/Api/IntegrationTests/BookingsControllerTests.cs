using System.Net;
using System.Net.Http.Json;
using ConferenceRoomBooking.Application.DTOs.Bookings;
using ConferenceRoomBooking.Domain.Entities;
using ConferenceRoomBooking.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ConferenceRoomBooking.Tests.Api.IntegrationTests;

/// <summary>
/// Integration-тести POST /api/bookings. На відміну від
/// Infrastructure.Services.BookingServiceTests (unit, сервіс напряму), тут
/// перевіряється увесь HTTP-конвеєр: ValidationFilter → BookingsController →
/// BookingService → GlobalExceptionHandler (мапінг доменних винятків у
/// коректні коди статусу й ProblemDetails).
/// </summary>
public class BookingsControllerTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public BookingsControllerTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturn201_WithCorrectlyCalculatedPrice()
    {
        var room = new ConferenceRoom
        { Name = "Зал А", Capacity = 50, BaseHourlyRate = 2000m, IsActive = true };
        var projector = new Service
        { Name = "Проєктор", Price = 500m, IsActive = true };

        await _factory.SeedAsync(ctx =>
        {
            ctx.ConferenceRooms.Add(room);
            ctx.Services.Add(projector);
        });

        // 10:00–12:00 повністю в межах Standard-зони (09:00–12:00, без
        // коефіцієнта) — очікувана ціна: 2000 * 2 години + 500 (послуга) = 4500.
        var startTime = DateTime.Today.AddDays(1).AddHours(10);
        var endTime = startTime.AddHours(2);

        var request = new CreateBookingRequest
        {
            ConferenceRoomId = room.Id,
            StartTime = startTime,
            EndTime = endTime,
            ServiceIds = new List<int> { projector.Id }
        };

        var response = await _client.PostAsJsonAsync("/api/bookings", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var booking = await response.Content.ReadFromJsonAsync<BookingResponse>();

        Assert.NotNull(booking);
        Assert.Equal(room.Id, booking!.ConferenceRoomId);
        Assert.Equal(room.Name, booking.ConferenceRoomName);
        Assert.Equal(4500m, booking.TotalPrice);
        Assert.Equal(nameof(BookingStatus.Confirmed), booking.Status);
        var includedService = Assert.Single(booking.Services);
        Assert.Equal(projector.Id, includedService.ServiceId);
    }

    [Fact]
    public async Task Create_ForConflictingInterval_ShouldReturn409()
    {
        var room = new ConferenceRoom
        { Name = "Зал А", Capacity = 50, BaseHourlyRate = 2000m, IsActive = true };

        var existingStart = DateTime.Today.AddDays(1).AddHours(10);
        var existingEnd = existingStart.AddHours(2); // 10:00–12:00 завтра

        await _factory.SeedAsync(ctx =>
        {
            ctx.ConferenceRooms.Add(room);
            ctx.Bookings.Add(new Booking
            {
                ConferenceRoomId = room.Id,
                ConferenceRoom = room,
                StartTime = existingStart,
                EndTime = existingEnd,
                TotalPrice = 4000m,
                Status = BookingStatus.Confirmed
            });
        });

        // 11:00–13:00 перетинається з існуючим 10:00–12:00.
        var request = new CreateBookingRequest
        {
            ConferenceRoomId = room.Id,
            StartTime = existingStart.AddHours(1),
            EndTime = existingEnd.AddHours(1)
        };

        var response = await _client.PostAsJsonAsync("/api/bookings", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status409Conflict, problem!.Status);
    }

    [Fact]
    public async Task Create_ForNonexistentRoom_ShouldReturn404()
    {
        var request = new CreateBookingRequest
        {
            ConferenceRoomId = 999_999, // в базі такого залу немає
            StartTime = DateTime.Today.AddDays(1).AddHours(10),
            EndTime = DateTime.Today.AddDays(1).AddHours(11)
        };

        var response = await _client.PostAsJsonAsync("/api/bookings", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
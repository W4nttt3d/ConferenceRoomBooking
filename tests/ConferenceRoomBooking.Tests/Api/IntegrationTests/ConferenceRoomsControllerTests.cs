using System.Net;
using System.Net.Http.Json;
using ConferenceRoomBooking.Application.DTOs.ConferenceRooms;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ConferenceRoomBooking.Tests.Api.IntegrationTests;

/// <summary>
/// Integration-тести POST /api/conference-rooms через реальний HTTP-конвеєр
/// (routing → ValidationFilter → controller → GlobalExceptionHandler), а не
/// виклик сервісу напряму, як у Infrastructure.Services.*Tests.
/// </summary>
public class ConferenceRoomsControllerTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public ConferenceRoomsControllerTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturn201_WithLocationHeaderAndCreatedRoom()
    {
        var request = new CreateConferenceRoomRequest
        {
            Name = "Зал Тест",
            Capacity = 25,
            BaseHourlyRate = 1500m
        };

        var response = await _client.PostAsJsonAsync("/api/conference-rooms", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var room = await response.Content.ReadFromJsonAsync<ConferenceRoomResponse>();

        Assert.NotNull(room);
        Assert.True(room!.Id > 0);
        Assert.Equal(request.Name, room.Name);
        Assert.Equal(request.Capacity, room.Capacity);
        Assert.Equal(request.BaseHourlyRate, room.BaseHourlyRate);
        // Новостворений зал завжди активний — soft delete вмикається окремим DELETE.
        Assert.True(room.IsActive);

        // Location має вести на GetById і фактично повертати той самий ресурс.
        var getResponse = await _client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidData_ShouldReturn400_WithValidationProblemDetails()
    {
        // Порожня назва (NotEmpty) і нульова місткість (GreaterThan(0)) —
        // обидва правила з CreateConferenceRoomRequestValidator мають
        // спрацювати одночасно.
        var request = new CreateConferenceRoomRequest
        {
            Name = string.Empty,
            Capacity = 0,
            BaseHourlyRate = 1000m
        };

        var response = await _client.PostAsJsonAsync("/api/conference-rooms", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Contains("Name", problem!.Errors.Keys);
        Assert.Contains("Capacity", problem.Errors.Keys);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}

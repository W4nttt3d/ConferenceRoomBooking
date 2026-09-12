using ConferenceRoomBooking.Application.DTOs.ConferenceRooms;
using ConferenceRoomBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBooking.Api.Controllers;

[ApiController]
[Route("api/conference-rooms")]
[Produces("application/json")]
public class ConferenceRoomsController : ControllerBase
{
    private readonly IConferenceRoomService _roomService;

    public ConferenceRoomsController(IConferenceRoomService roomService)
    {
        _roomService = roomService;
    }

    /// <summary>Отримати список усіх залів (включно з неактивними).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ConferenceRoomResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ConferenceRoomResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var rooms = await _roomService.GetAllAsync(cancellationToken);
        return Ok(rooms);
    }

    /// <summary>
    /// Пошук активних залів із достатньою місткістю, вільних на заданий інтервал.
    /// Приклад: /api/conference-rooms/available?startTime=2026-09-15T10:00&amp;endTime=2026-09-15T14:00&amp;capacity=50
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(IReadOnlyList<ConferenceRoomResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ConferenceRoomResponse>>> GetAvailable(
        [FromQuery] AvailableRoomsQuery query,
        CancellationToken cancellationToken)
    {
        var rooms = await _roomService.SearchAvailableAsync(query.StartTime, query.EndTime, query.Capacity, cancellationToken);
        return Ok(rooms);
    }

    /// <summary>Отримати зал за Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ConferenceRoomResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConferenceRoomResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var room = await _roomService.GetByIdAsync(id, cancellationToken);

        if (room is null)
        {
            return NotFound(new { message = $"Зал з Id = {id} не знайдено." });
        }

        return Ok(room);
    }

    /// <summary>Створити новий зал.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ConferenceRoomResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConferenceRoomResponse>> Create(
        CreateConferenceRoomRequest request,
        CancellationToken cancellationToken)
    {
        var room = await _roomService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    /// <summary>Редагувати існуючий зал.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ConferenceRoomResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConferenceRoomResponse>> Update(
        int id,
        UpdateConferenceRoomRequest request,
        CancellationToken cancellationToken)
    {
        var room = await _roomService.UpdateAsync(id, request, cancellationToken);

        if (room is null)
        {
            return NotFound(new { message = $"Зал з Id = {id} не знайдено." });
        }

        return Ok(room);
    }

    /// <summary>
    /// "Видалити" зал — насправді soft delete (IsActive = false).
    /// Фізичне видалення не використовується, щоб не втратити історію бронювань.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var deactivated = await _roomService.DeactivateAsync(id, cancellationToken);

        if (!deactivated)
        {
            return NotFound(new { message = $"Зал з Id = {id} не знайдено." });
        }

        return NoContent();
    }
}

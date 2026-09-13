using ConferenceRoomBooking.Application.DTOs.Bookings;
using ConferenceRoomBooking.Application.Exceptions;
using ConferenceRoomBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBooking.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>Отримати список усіх бронювань (найновіші спочатку).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BookingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var bookings = await _bookingService.GetAllAsync(cancellationToken);
        return Ok(bookings);
    }

    /// <summary>Отримати бронювання за Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.GetByIdAsync(id, cancellationToken);

        if (booking is null)
        {
            return NotFound(new { message = $"Бронювання з Id = {id} не знайдено." });
        }

        return Ok(booking);
    }

    /// <summary>
    /// Створити бронювання. Перевіряє: зал існує й активний, немає конфлікту
    /// за часом, вказані послуги існують і активні; розраховує вартість
    /// (оренда залу за тарифними зонами + послуги) і зберігає результат.
    /// </summary>
    /// <remarks>
    /// try/catch тут — тимчасове рішення. На кроці 13 плану з'явиться
    /// глобальний exception-handling middleware, і цей блок піде звідси,
    /// а винятки (NotFoundException, BookingConflictException,
    /// UnprocessableEntityException) далі кидатимуться так само з сервісу,
    /// але оброблятимуться в одному місці для всіх контролерів одразу.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BookingResponse>> Create(
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _bookingService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BookingConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnprocessableEntityException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
    }
}

using ConferenceRoomBooking.Application.DTOs.Bookings;
using ConferenceRoomBooking.Application.Exceptions;
using ConferenceRoomBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBooking.Api.Controllers;

/// <summary>Бронювання конференц-залів і розрахунок вартості оренди.</summary>
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
    /// <response code="200">Список бронювань.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BookingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var bookings = await _bookingService.GetAllAsync(cancellationToken);
        return Ok(bookings);
    }

    /// <summary>Отримати бронювання за Id.</summary>
    /// <param name="id">Id бронювання.</param>
    /// <response code="200">Бронювання знайдено.</response>
    /// <response code="404">Бронювання з таким Id не існує.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.GetByIdAsync(id, cancellationToken);

        if (booking is null)
        {
            throw new NotFoundException($"Бронювання з Id = {id} не знайдено.");
        }

        return Ok(booking);
    }

    /// <summary>
    /// Створити бронювання. Перевіряє: зал існує й активний, немає конфлікту
    /// за часом, вказані послуги існують і активні; розраховує вартість
    /// (оренда залу за тарифними зонами + послуги) і зберігає результат.
    /// Винятки (NotFoundException, BookingConflictException,
    /// UnprocessableEntityException), кинуті сервісом, обробляються
    /// глобальним GlobalExceptionHandler — тут окремого try/catch не потрібно.
    /// </summary>
    /// <param name="request">Зал, інтервал часу та обрані послуги.</param>
    /// <response code="201">Бронювання створено; у відповіді — розрахована вартість.</response>
    /// <response code="400">Дані не пройшли валідацію (наприклад, час у минулому, поза робочими годинами 06:00–23:00, або startTime пізніше endTime).</response>
    /// <response code="404">Зал або одна з обраних послуг не знайдені.</response>
    /// <response code="409">Зал уже заброньований на цей інтервал (перетин часу з існуючим бронюванням).</response>
    /// <response code="422">Одна з обраних послуг деактивована й недоступна для бронювання.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BookingResponse>> Create(
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }
}

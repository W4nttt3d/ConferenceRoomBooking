using ConferenceRoomBooking.Application.DTOs.Services;
using ConferenceRoomBooking.Application.Exceptions;
using ConferenceRoomBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBooking.Api.Controllers;

/// <summary>Управління додатковими послугами (проєктор, Wi-Fi, звук тощо), які можна замовити разом із бронюванням.</summary>
[ApiController]
[Route("api/services")]
[Produces("application/json")]
public class ServicesController : ControllerBase
{
    private readonly IServiceService _serviceService;

    public ServicesController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    /// <summary>Отримати список усіх послуг (включно з неактивними).</summary>
    /// <response code="200">Список послуг.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var services = await _serviceService.GetAllAsync(cancellationToken);
        return Ok(services);
    }

    /// <summary>Отримати послугу за Id.</summary>
    /// <param name="id">Id послуги.</param>
    /// <response code="200">Послугу знайдено.</response>
    /// <response code="404">Послугу з таким Id не існує.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var service = await _serviceService.GetByIdAsync(id, cancellationToken);

        if (service is null)
        {
            throw new NotFoundException($"Послугу з Id = {id} не знайдено.");
        }

        return Ok(service);
    }

    /// <summary>Створити нову послугу.</summary>
    /// <param name="request">Назва та вартість нової послуги.</param>
    /// <response code="201">Послугу створено; у заголовку Location — посилання на новий ресурс.</response>
    /// <response code="400">Дані не пройшли валідацію (наприклад, порожня назва або нульова ціна).</response>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceResponse>> Create(
        CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var service = await _serviceService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
    }

    /// <summary>Редагувати існуючу послугу.</summary>
    /// <param name="id">Id послуги.</param>
    /// <param name="request">Оновлені назва та вартість.</param>
    /// <response code="200">Послугу оновлено.</response>
    /// <response code="400">Дані не пройшли валідацію.</response>
    /// <response code="404">Послугу з таким Id не існує.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResponse>> Update(
        int id,
        UpdateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var service = await _serviceService.UpdateAsync(id, request, cancellationToken);

        if (service is null)
        {
            throw new NotFoundException($"Послугу з Id = {id} не знайдено.");
        }

        return Ok(service);
    }

    /// <summary>"Видалити" послугу — насправді soft delete (IsActive = false).</summary>
    /// <param name="id">Id послуги.</param>
    /// <response code="204">Послугу деактивовано.</response>
    /// <response code="404">Послугу з таким Id не існує.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var deactivated = await _serviceService.DeactivateAsync(id, cancellationToken);

        if (!deactivated)
        {
            throw new NotFoundException($"Послугу з Id = {id} не знайдено.");
        }

        return NoContent();
    }
}

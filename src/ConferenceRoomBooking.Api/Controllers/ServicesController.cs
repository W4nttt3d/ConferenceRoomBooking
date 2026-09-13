using ConferenceRoomBooking.Application.DTOs.Services;
using ConferenceRoomBooking.Application.Exceptions;
using ConferenceRoomBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBooking.Api.Controllers;

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
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var services = await _serviceService.GetAllAsync(cancellationToken);
        return Ok(services);
    }

    /// <summary>Отримати послугу за Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [HttpPost]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceResponse>> Create(
        CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var service = await _serviceService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
    }

    /// <summary>Редагувати існуючу послугу.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

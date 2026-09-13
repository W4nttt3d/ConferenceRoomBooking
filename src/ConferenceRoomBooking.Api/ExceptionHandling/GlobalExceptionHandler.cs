using ConferenceRoomBooking.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBooking.Api.ExceptionHandling;

/// <summary>
/// Єдина точка обробки необроблених винятків для всього API. Клієнту
/// ніколи не потрапляють StackTrace, SQL-помилки чи інші внутрішні деталі -
/// вони йдуть тільки в лог; у відповідь - лише ProblemDetails (RFC 7807)
/// з безпечним для показу повідомленням.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Ресурс не знайдено"),
            BookingConflictException => (StatusCodes.Status409Conflict, "Конфлікт бронювання"),
            UnprocessableEntityException => (StatusCodes.Status422UnprocessableEntity, "Неможливо обробити запит"),
            _ => (StatusCodes.Status500InternalServerError, "Внутрішня помилка сервера")
        };

        var isUnexpected = statusCode == StatusCodes.Status500InternalServerError;

        if (isUnexpected)
        {
            // Повний виняток - тільки в лог, ніколи в HTTP-відповідь.
            _logger.LogError(exception, "Необроблений виняток під час {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "{ExceptionType}: {Message}",
                exception.GetType().Name, exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            // Для доменних винятків (404/409/422) повідомлення вже сформульоване
            // для користувача в сервісному шарі. Для 500 - тільки загальний текст.
            Detail = isUnexpected
                ? "Сталася непередбачена помилка. Спробуйте пізніше."
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
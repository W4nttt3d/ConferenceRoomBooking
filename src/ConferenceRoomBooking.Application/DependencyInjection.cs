using ConferenceRoomBooking.Application.Validators.ConferenceRooms;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceRoomBooking.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Реєструє все, що належить Application-шару: наразі — усі FluentValidation
    /// валідатори, знайдені в цій збірці. Викликається з Program.cs одним рядком:
    /// builder.Services.AddApplication();
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateConferenceRoomRequestValidator>();

        return services;
    }
}

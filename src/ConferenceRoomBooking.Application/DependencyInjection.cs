using ConferenceRoomBooking.Application.Interfaces;
using ConferenceRoomBooking.Application.Services;
using ConferenceRoomBooking.Application.Validators.ConferenceRooms;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceRoomBooking.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Реєструє все, що належить Application-шару: FluentValidation
    /// валідатори (знайдені в цій збірці) та PricingService. Викликається
    /// з Program.cs одним рядком: builder.Services.AddApplication();
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateConferenceRoomRequestValidator>();

        // PricingService не має жодного стану — безпечно як Singleton.
        services.AddSingleton<IPricingService, PricingService>();

        return services;
    }
}

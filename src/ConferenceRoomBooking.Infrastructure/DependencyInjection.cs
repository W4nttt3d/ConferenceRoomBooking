using ConferenceRoomBooking.Application.Interfaces;
using ConferenceRoomBooking.Infrastructure.Data;
using ConferenceRoomBooking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceRoomBooking.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Реєструє все, що належить Infrastructure-шару (наразі — AppDbContext).
    /// Викликається з Program.cs одним рядком: builder.Services.AddInfrastructure(builder.Configuration);
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' не знайдено в конфігурації.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IConferenceRoomService, ConferenceRoomService>();

        return services;
    }
}

using ConferenceRoomBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBooking.Infrastructure.Data;

/// <summary>
/// Наповнює базу початковими даними (зали, послуги), щоб API можна було
/// одразу тестувати після запуску, без ручного заповнення через Swagger.
/// Ідемпотентний: якщо зали вже є в базі, нічого не робить — можна
/// безпечно викликати щоразу при старті додатку.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Якщо хоч один зал уже є — вважаємо, що seed уже виконано раніше.
        if (await context.ConferenceRooms.AnyAsync())
        {
            return;
        }

        var rooms = new List<ConferenceRoom>
        {
            new() { Name = "Зал А", Capacity = 50, BaseHourlyRate = 2000m, IsActive = true },
            new() { Name = "Зал B", Capacity = 100, BaseHourlyRate = 3500m, IsActive = true },
            new() { Name = "Зал C", Capacity = 30, BaseHourlyRate = 1500m, IsActive = true }
        };

        var services = new List<Service>
        {
            new() { Name = "Проєктор", Price = 500m, IsActive = true },
            new() { Name = "Wi-Fi", Price = 300m, IsActive = true },
            new() { Name = "Звук", Price = 700m, IsActive = true }
        };

        await context.ConferenceRooms.AddRangeAsync(rooms);
        await context.Services.AddRangeAsync(services);

        await context.SaveChangesAsync();
    }
}

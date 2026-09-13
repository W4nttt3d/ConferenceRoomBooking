using System.Linq;
using ConferenceRoomBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceRoomBooking.Tests.Api.IntegrationTests;

/// <summary>
/// Піднімає весь застосунок (Program.cs, включно з роутингом, фільтрами
/// валідації та GlobalExceptionHandler) в пам'яті.
///
/// Кожен тестовий клас створює власний екземпляр цієї фабрики (у
/// конструкторі, без IClassFixture) — xUnit створює новий екземпляр класу
/// на кожен [Fact], тому кожен тест автоматично отримує свіжу, повністю
/// ізольовану in-memory базу.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }

    /// <summary>
    /// Готує дані (Arrange) напряму через AppDbContext у власному DI-scope,
    /// без зайвого HTTP round-trip. SaveChangesAsync викликається один раз
    /// в кінці — action лише додає/змінює сутності в контексті.
    /// </summary>
    public async Task SeedAsync(Action<AppDbContext> seedAction)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        seedAction(context);
        await context.SaveChangesAsync();
    }
}

using System.Linq;
using ConferenceRoomBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceRoomBooking.Tests.Api.IntegrationTests;

/// <summary>
/// Піднімає весь застосунок (Program.cs, включно з роутингом, фільтрами
/// валідації та GlobalExceptionHandler) в пам'яті, як того вимагає крок 17
/// плану ("Integration tests ... через WebApplicationFactory").
///
/// Два свідомих рішення:
/// 1. AppDbContext, зареєстрований в Infrastructure.DependencyInjection з
///    UseNpgsql, тут підмінюється на EF Core InMemory з унікальною назвою
///    бази (Guid) — щоб тести не залежали від реального PostgreSQL і не
///    впливали одне на одного.
/// 2. Environment = "Testing" (не "Development") — щоб не спрацював
///    автоматичний DbInitializer.SeedAsync у Program.cs (він умовний на
///    IsDevelopment()). Кожен тест сам готує собі дані через SeedAsync
///    нижче — так набір даних завжди точно відомий і не залежить від
///    порядку виконання тестів.
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

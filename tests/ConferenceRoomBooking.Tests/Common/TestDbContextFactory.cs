using ConferenceRoomBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBooking.Tests.Common;

/// <summary>
/// Кожен виклик створює повністю ізольовану in-memory базу (унікальна назва
/// через Guid), щоб тести не впливали одне на одного та могли виконуватись
/// паралельно.
/// </summary>
public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}

using ConferenceRoomBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBooking.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ConferenceRoom> ConferenceRooms => Set<ConferenceRoom>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingService> BookingServices => Set<BookingService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Підхоплює всі IEntityTypeConfiguration<T> з цієї збірки
        // (папка Data/Configurations) — не потрібно реєструвати їх вручну.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

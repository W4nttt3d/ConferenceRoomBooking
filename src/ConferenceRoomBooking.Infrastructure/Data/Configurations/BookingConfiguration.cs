using ConferenceRoomBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRoomBooking.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.TotalPrice)
            .HasColumnType("numeric(10,2)");

        builder.Property(b => b.Status)
            .HasConversion<string>()   // зберігати enum як текст, а не int - читабельніше в БД напряму
            .HasMaxLength(20);

        // Сервіс односайтовий (один зал, один часовий пояс), тому час
        // зберігається без прив'язки до зони - "timestamp without time zone".
        // Npgsql 8+ вимагає точної відповідності Kind і типу колонки,
        // інакше кидає ArgumentException.
        builder.Property(b => b.StartTime)
            .HasColumnType("timestamp without time zone");

        builder.Property(b => b.EndTime)
            .HasColumnType("timestamp without time zone");

        builder.Property(b => b.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .HasDefaultValueSql("now()");

        // Пошук доступних залів фільтрує по ConferenceRoomId і порівнює
        // StartTime/EndTime - цей індекс покриває той запит.
        builder.HasIndex(b => new { b.ConferenceRoomId, b.StartTime, b.EndTime });

        builder.HasOne(b => b.ConferenceRoom)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.ConferenceRoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
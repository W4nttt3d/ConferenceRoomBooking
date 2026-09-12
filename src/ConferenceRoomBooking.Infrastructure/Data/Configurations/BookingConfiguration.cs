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
            .HasConversion<string>()   // зберігати enum як текст, а не int — читабельніше в БД напряму
            .HasMaxLength(20);

        builder.Property(b => b.CreatedAt)
            .HasDefaultValueSql("now()");

        // Перевірка конфлікту бронювань (крок 5 плану) щоразу фільтрує
        // по ConferenceRoomId і порівнює StartTime/EndTime — цей індекс
        // покриває найчастіший запит availability search.
        builder.HasIndex(b => new { b.ConferenceRoomId, b.StartTime, b.EndTime });

        builder.HasOne(b => b.ConferenceRoom)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.ConferenceRoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

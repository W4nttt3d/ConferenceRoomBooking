using ConferenceRoomBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRoomBooking.Infrastructure.Data.Configurations;

public class BookingServiceConfiguration : IEntityTypeConfiguration<Domain.Entities.BookingService>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.BookingService> builder)
    {
        builder.ToTable("BookingServices");

        // Складений первинний ключ: одна й та сама послуга не може бути
        // додана до одного бронювання двічі окремими рядками.
        builder.HasKey(bs => new { bs.BookingId, bs.ServiceId });

        builder.Property(bs => bs.Price)
            .HasColumnType("numeric(10,2)");

        builder.HasOne(bs => bs.Booking)
            .WithMany(b => b.BookingServices)
            .HasForeignKey(bs => bs.BookingId)
            .OnDelete(DeleteBehavior.Cascade); // видалення бронювання видаляє його рядки послуг

        builder.HasOne(bs => bs.Service)
            .WithMany(s => s.BookingServices)
            .HasForeignKey(bs => bs.ServiceId)
            .OnDelete(DeleteBehavior.Restrict); // не можна видалити Service, поки є бронювання з ним
    }
}

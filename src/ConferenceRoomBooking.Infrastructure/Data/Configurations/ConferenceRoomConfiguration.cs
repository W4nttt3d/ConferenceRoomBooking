using ConferenceRoomBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRoomBooking.Infrastructure.Data.Configurations;

public class ConferenceRoomConfiguration : IEntityTypeConfiguration<ConferenceRoom>
{
    public void Configure(EntityTypeBuilder<ConferenceRoom> builder)
    {
        builder.ToTable("ConferenceRooms");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.BaseHourlyRate)
            .HasColumnType("numeric(10,2)");

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);

        // Пошук доступних залів завжди фільтрує по IsActive + Capacity —
        // складений індекс пришвидшить цей найчастіший запит.
        builder.HasIndex(r => new { r.IsActive, r.Capacity });

        builder.HasMany(r => r.Bookings)
            .WithOne(b => b.ConferenceRoom)
            .HasForeignKey(b => b.ConferenceRoomId)
            .OnDelete(DeleteBehavior.Restrict); // soft delete замість каскадного видалення
    }
}

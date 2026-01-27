using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentADad.Domain.Bookings;

namespace RentADad.Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");
        builder.HasKey(booking => booking.Id);

        builder.Property(booking => booking.JobId).IsRequired();
        builder.Property(booking => booking.ProviderId).IsRequired();
        builder.Property(booking => booking.StartUtc).IsRequired();
        builder.Property(booking => booking.EndUtc).IsRequired();
        builder.Property(booking => booking.Status).HasConversion<string>().IsRequired();
        builder.Property(booking => booking.UpdatedUtc).IsRequired().IsConcurrencyToken();

        builder.HasIndex(booking => booking.JobId);
        builder.HasIndex(booking => booking.ProviderId);
        builder.HasIndex(booking => booking.Status);
        builder.HasIndex(booking => new { booking.ProviderId, booking.StartUtc, booking.EndUtc })
            .HasDatabaseName("IX_bookings_provider_window");
    }
}

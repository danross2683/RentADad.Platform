using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RentADad.Infrastructure.Persistence.Configurations;

public sealed class JobListingConfiguration : IEntityTypeConfiguration<JobListing>
{
    public void Configure(EntityTypeBuilder<JobListing> builder)
    {
        builder.ToTable("job_listings");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.CustomerId).IsRequired();
        builder.Property(item => item.Location).HasMaxLength(256).IsRequired();
        builder.Property(item => item.Status).HasMaxLength(32).IsRequired();
        builder.Property(item => item.ActiveBookingId);
        builder.Property(item => item.UpdatedUtc).IsRequired();

        builder.Property(item => item.ServiceIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Guid[]>(v) ?? Array.Empty<Guid>())
            .HasColumnType("text");

        builder.HasIndex(item => item.CustomerId);
        builder.HasIndex(item => item.Status);
        builder.HasIndex(item => item.UpdatedUtc);
    }
}

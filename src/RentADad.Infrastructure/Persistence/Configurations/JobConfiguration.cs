using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentADad.Domain.Jobs;

namespace RentADad.Infrastructure.Persistence.Configurations;

public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("jobs");
        builder.HasKey(job => job.Id);

        builder.Property(job => job.CustomerId).IsRequired();
        builder.Property(job => job.Location).HasMaxLength(256).IsRequired();
        builder.Property(job => job.Status).HasConversion<string>().IsRequired();
        builder.Property(job => job.ActiveBookingId);

        builder.OwnsMany(
            job => job.Services,
            services =>
            {
                services.ToTable("job_services");
                services.WithOwner().HasForeignKey("JobId");
                services.HasKey(s => s.Id);
                services.Property(s => s.ServiceId).IsRequired();
            });

        builder.Navigation(job => job.Services).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentADad.Domain.Providers;

namespace RentADad.Infrastructure.Persistence.Configurations;

public sealed class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("providers");
        builder.HasKey(provider => provider.Id);

        builder.Property(provider => provider.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(provider => provider.UpdatedUtc).IsRequired().IsConcurrencyToken();

        builder.OwnsMany(
            provider => provider.Availabilities,
            availabilities =>
            {
                availabilities.ToTable("provider_availability");
                availabilities.WithOwner().HasForeignKey("ProviderId");
                availabilities.HasKey(a => a.Id);
                availabilities.Property(a => a.Id).ValueGeneratedNever();
                availabilities.Property(a => a.StartUtc).IsRequired();
                availabilities.Property(a => a.EndUtc).IsRequired();
                availabilities.HasIndex("ProviderId", "StartUtc", "EndUtc")
                    .HasDatabaseName("IX_provider_availability_provider_window");
            });

        builder.Navigation(provider => provider.Availabilities).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

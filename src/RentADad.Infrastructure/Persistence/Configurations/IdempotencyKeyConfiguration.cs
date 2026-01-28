using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RentADad.Infrastructure.Persistence.Configurations;

public sealed class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
{
    public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
    {
        builder.ToTable("idempotency_keys");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Key).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Method).HasMaxLength(16).IsRequired();
        builder.Property(item => item.Path).HasMaxLength(512).IsRequired();
        builder.Property(item => item.RequestHash).HasMaxLength(128).IsRequired();
        builder.Property(item => item.ResponseBody).IsRequired();
        builder.Property(item => item.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(item => item.ResponseStatusCode).IsRequired();
        builder.Property(item => item.CreatedUtc).IsRequired();
        builder.Property(item => item.ExpiresUtc).IsRequired();

        builder.HasIndex(item => new { item.Key, item.Method, item.Path }).IsUnique();
        builder.HasIndex(item => item.ExpiresUtc);
    }
}

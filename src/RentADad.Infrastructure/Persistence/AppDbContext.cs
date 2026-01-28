using Microsoft.EntityFrameworkCore;
using RentADad.Domain.Bookings;
using RentADad.Domain.Jobs;
using RentADad.Domain.Providers;

namespace RentADad.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();
    public DbSet<JobListing> JobListings => Set<JobListing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ApplyUpdatedUtc();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyUpdatedUtc();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyUpdatedUtc()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified)) continue;

            if (entry.Entity is Job or Booking or Provider)
            {
                entry.Property("UpdatedUtc").CurrentValue = utcNow;
            }
        }
    }
}

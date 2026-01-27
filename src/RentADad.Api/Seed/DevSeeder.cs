using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentADad.Domain.Bookings;
using RentADad.Domain.Jobs;
using RentADad.Domain.Providers;
using RentADad.Infrastructure.Persistence;

namespace RentADad.Api.Seed;

public sealed class DevSeeder
{
    public async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Jobs.AnyAsync(cancellationToken)) return;

        var provider = new Provider(Guid.NewGuid(), "Alex Fixer");
        provider.AddAvailability(DateTime.UtcNow.Date.AddDays(1).AddHours(9), DateTime.UtcNow.Date.AddDays(1).AddHours(17));

        var job = new Job(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "123 Main St",
            new List<Guid> { Guid.NewGuid() });

        var booking = new Booking(
            Guid.NewGuid(),
            job.Id,
            provider.Id,
            DateTime.UtcNow.Date.AddDays(1).AddHours(10),
            DateTime.UtcNow.Date.AddDays(1).AddHours(12));

        dbContext.Providers.Add(provider);
        dbContext.Jobs.Add(job);
        dbContext.Bookings.Add(booking);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

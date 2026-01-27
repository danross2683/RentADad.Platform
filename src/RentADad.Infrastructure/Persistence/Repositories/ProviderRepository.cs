using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentADad.Application.Abstractions.Repositories;
using RentADad.Domain.Providers;

namespace RentADad.Infrastructure.Persistence.Repositories;

public sealed class ProviderRepository : IProviderRepository
{
    private readonly AppDbContext _dbContext;

    public ProviderRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Provider?> GetByIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Providers
            .AsNoTracking()
            .Include(provider => provider.Availabilities)
            .FirstOrDefaultAsync(provider => provider.Id == providerId, cancellationToken);
    }

    public Task<Provider?> GetForUpdateAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Providers
            .Include(provider => provider.Availabilities)
            .FirstOrDefaultAsync(provider => provider.Id == providerId, cancellationToken);
    }

    public void Add(Provider provider)
    {
        _dbContext.Providers.Add(provider);
    }
}

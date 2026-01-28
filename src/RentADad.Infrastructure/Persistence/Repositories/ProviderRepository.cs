using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentADad.Application.Common.Paging;
using RentADad.Application.Providers.Requests;
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

    public async Task<PagedResult<Provider>> ListAsync(ProviderListQuery query, CancellationToken cancellationToken = default)
    {
        var providers = _dbContext.Providers
            .AsNoTracking()
            .Include(provider => provider.Availabilities)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.DisplayNameContains))
        {
            var term = query.DisplayNameContains.Trim();
            var lowered = term.ToLower();
            providers = providers.Where(provider =>
                EF.Functions.Like(provider.DisplayName.ToLower(), $"%{lowered}%"));
        }

        var total = await providers.CountAsync(cancellationToken);
        var items = await providers
            .OrderByDescending(provider => provider.UpdatedUtc)
            .ThenBy(provider => provider.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Provider>(items, query.Page, query.PageSize, total);
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

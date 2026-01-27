using System;
using System.Threading;
using System.Threading.Tasks;
using RentADad.Application.Common.Paging;
using RentADad.Application.Providers.Requests;
using RentADad.Domain.Providers;

namespace RentADad.Application.Abstractions.Repositories;

public interface IProviderRepository
{
    Task<PagedResult<Provider>> ListAsync(ProviderListQuery query, CancellationToken cancellationToken = default);
    Task<Provider?> GetByIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<Provider?> GetForUpdateAsync(Guid providerId, CancellationToken cancellationToken = default);
    void Add(Provider provider);
}

using System;
using System.Threading;
using System.Threading.Tasks;
using RentADad.Domain.Providers;

namespace RentADad.Application.Abstractions.Repositories;

public interface IProviderRepository
{
    Task<Provider?> GetByIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<Provider?> GetForUpdateAsync(Guid providerId, CancellationToken cancellationToken = default);
    void Add(Provider provider);
}

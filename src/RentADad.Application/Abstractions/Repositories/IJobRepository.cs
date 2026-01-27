using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RentADad.Application.Common.Paging;
using RentADad.Application.Jobs.Requests;
using RentADad.Domain.Jobs;

namespace RentADad.Application.Abstractions.Repositories;

public interface IJobRepository
{
    Task<List<Job>> ListAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<Job>> ListAsync(JobListQuery query, CancellationToken cancellationToken = default);
    Task<Job?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<Job?> GetForUpdateAsync(Guid jobId, CancellationToken cancellationToken = default);
    void Add(Job job);
}

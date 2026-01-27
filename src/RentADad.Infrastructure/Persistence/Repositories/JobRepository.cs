using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentADad.Application.Abstractions.Repositories;
using RentADad.Domain.Jobs;

namespace RentADad.Infrastructure.Persistence.Repositories;

public sealed class JobRepository : IJobRepository
{
    private readonly AppDbContext _dbContext;

    public JobRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Job>> ListAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Jobs
            .AsNoTracking()
            .Include(job => job.Services)
            .ToListAsync(cancellationToken);
    }

    public Task<Job?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Jobs
            .AsNoTracking()
            .Include(job => job.Services)
            .FirstOrDefaultAsync(job => job.Id == jobId, cancellationToken);
    }

    public Task<Job?> GetForUpdateAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Jobs
            .Include(job => job.Services)
            .FirstOrDefaultAsync(job => job.Id == jobId, cancellationToken);
    }

    public void Add(Job job)
    {
        _dbContext.Jobs.Add(job);
    }
}

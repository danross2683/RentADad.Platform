using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentADad.Application.Abstractions.ReadModels;
using RentADad.Application.Common.Paging;
using RentADad.Application.Jobs.ReadModels;
using RentADad.Application.Jobs.Requests;

namespace RentADad.Infrastructure.Persistence.Repositories;

public sealed class JobListingRepository : IJobListingReader, IJobListingWriter
{
    private readonly AppDbContext _dbContext;

    public JobListingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<JobListingRow>> ListAsync(JobListQuery query, CancellationToken cancellationToken = default)
    {
        var listings = _dbContext.JobListings.AsNoTracking().AsQueryable();

        if (query.CustomerId is not null)
        {
            listings = listings.Where(item => item.CustomerId == query.CustomerId);
        }

        if (query.Status is not null)
        {
            listings = listings.Where(item => item.Status == query.Status.ToString());
        }

        var total = await listings.CountAsync(cancellationToken);
        var items = await listings
            .OrderByDescending(item => item.UpdatedUtc)
            .ThenBy(item => item.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(item => new JobListingRow(
                item.Id,
                item.CustomerId,
                item.Location,
                item.ServiceIds,
                item.Status,
                item.ActiveBookingId,
                item.UpdatedUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<JobListingRow>(items, query.Page, query.PageSize, total);
    }

    public async Task UpsertAsync(JobListingWriteModel model, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.JobListings.FirstOrDefaultAsync(item => item.Id == model.Id, cancellationToken);
        var incoming = new JobListing(
            model.Id,
            model.CustomerId,
            model.Location,
            model.ServiceIds,
            model.Status,
            model.ActiveBookingId,
            model.UpdatedUtc);

        if (existing is null)
        {
            _dbContext.JobListings.Add(incoming);
        }
        else
        {
            existing.UpdateFrom(incoming);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

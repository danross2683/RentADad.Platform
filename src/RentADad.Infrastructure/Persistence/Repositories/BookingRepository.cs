using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentADad.Application.Bookings.Requests;
using RentADad.Application.Common.Paging;
using RentADad.Application.Abstractions.Repositories;
using RentADad.Domain.Bookings;

namespace RentADad.Infrastructure.Persistence.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _dbContext;

    public BookingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<Booking>> ListAsync(BookingListQuery query, CancellationToken cancellationToken = default)
    {
        var bookings = _dbContext.Bookings
            .AsNoTracking()
            .AsQueryable();

        if (query.JobId is not null)
        {
            bookings = bookings.Where(booking => booking.JobId == query.JobId);
        }

        if (query.ProviderId is not null)
        {
            bookings = bookings.Where(booking => booking.ProviderId == query.ProviderId);
        }

        if (query.Status is not null)
        {
            bookings = bookings.Where(booking => booking.Status == query.Status);
        }

        if (query.StartUtcFrom is not null)
        {
            bookings = bookings.Where(booking => booking.StartUtc >= query.StartUtcFrom);
        }

        if (query.StartUtcTo is not null)
        {
            bookings = bookings.Where(booking => booking.StartUtc <= query.StartUtcTo);
        }

        var total = await bookings.CountAsync(cancellationToken);
        var items = await bookings
            .OrderByDescending(booking => booking.UpdatedUtc)
            .ThenBy(booking => booking.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Booking>(items, query.Page, query.PageSize, total);
    }

    public async Task<List<Guid>> ListExpiredPendingAsync(DateTime utcNow, int batchSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bookings
            .AsNoTracking()
            .Where(booking => booking.Status == BookingStatus.Pending && booking.EndUtc <= utcNow)
            .OrderBy(booking => booking.EndUtc)
            .ThenBy(booking => booking.Id)
            .Select(booking => booking.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(booking => booking.Id == bookingId, cancellationToken);
    }

    public Task<Booking?> GetForUpdateAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Bookings
            .FirstOrDefaultAsync(booking => booking.Id == bookingId, cancellationToken);
    }

    public void Add(Booking booking)
    {
        _dbContext.Bookings.Add(booking);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

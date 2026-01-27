using System;
using System.Threading;
using System.Threading.Tasks;
using RentADad.Domain.Bookings;

namespace RentADad.Application.Abstractions.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<Booking?> GetForUpdateAsync(Guid bookingId, CancellationToken cancellationToken = default);
    void Add(Booking booking);
}

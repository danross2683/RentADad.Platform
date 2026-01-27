using System;
using System.Threading;
using System.Threading.Tasks;
using RentADad.Application.Bookings.Requests;
using RentADad.Application.Common.Paging;
using RentADad.Domain.Bookings;

namespace RentADad.Application.Abstractions.Repositories;

public interface IBookingRepository
{
    Task<PagedResult<Booking>> ListAsync(BookingListQuery query, CancellationToken cancellationToken = default);
    Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<Booking?> GetForUpdateAsync(Guid bookingId, CancellationToken cancellationToken = default);
    void Add(Booking booking);
}

using System;
using System.Threading;
using System.Threading.Tasks;
using RentADad.Application.Abstractions.Persistence;
using RentADad.Application.Abstractions.Repositories;
using RentADad.Application.Bookings.Requests;
using RentADad.Application.Bookings.Responses;
using RentADad.Domain.Bookings;
using RentADad.Domain.Common;

namespace RentADad.Application.Bookings;

public sealed class BookingService
{
    private readonly IBookingRepository _bookings;
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(IBookingRepository bookings, IUnitOfWork unitOfWork)
    {
        _bookings = bookings;
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingResponse?> GetAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookings.GetByIdAsync(bookingId, cancellationToken);
        return booking is null ? null : ToResponse(booking);
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var booking = new Booking(
                Guid.NewGuid(),
                request.JobId,
                request.ProviderId,
                request.StartUtc,
                request.EndUtc);

            _bookings.Add(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ToResponse(booking);
        }
        catch (DomainRuleViolationException ex)
        {
            throw new BookingDomainException(ex.Message);
        }
    }

    public Task<BookingResponse?> ConfirmAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return ApplyAction(bookingId, cancellationToken, booking => booking.Confirm());
    }

    public Task<BookingResponse?> DeclineAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return ApplyAction(bookingId, cancellationToken, booking => booking.Decline());
    }

    public Task<BookingResponse?> ExpireAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return ApplyAction(bookingId, cancellationToken, booking => booking.Expire());
    }

    public Task<BookingResponse?> CancelAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return ApplyAction(bookingId, cancellationToken, booking => booking.Cancel());
    }

    private async Task<BookingResponse?> ApplyAction(
        Guid bookingId,
        CancellationToken cancellationToken,
        Action<Booking> action)
    {
        var booking = await _bookings.GetForUpdateAsync(bookingId, cancellationToken);
        if (booking is null) return null;

        try
        {
            action(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ToResponse(booking);
        }
        catch (DomainRuleViolationException ex)
        {
            throw new BookingDomainException(ex.Message);
        }
    }

    private static BookingResponse ToResponse(Booking booking)
    {
        return new BookingResponse(
            booking.Id,
            booking.JobId,
            booking.ProviderId,
            booking.StartUtc,
            booking.EndUtc,
            booking.Status.ToString());
    }
}

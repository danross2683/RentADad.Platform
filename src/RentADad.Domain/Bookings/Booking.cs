using System;
using RentADad.Domain.Common;

namespace RentADad.Domain.Bookings;

public sealed class Booking
{
    private Booking()
    {
    }

    public Booking(Guid id, Guid jobId, Guid providerId, DateTime startUtc, DateTime endUtc)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("Booking id is required.");
        if (jobId == Guid.Empty) throw new DomainRuleViolationException("Job id is required.");
        if (providerId == Guid.Empty) throw new DomainRuleViolationException("Provider id is required.");
        if (endUtc <= startUtc) throw new DomainRuleViolationException("Booking end must be after start.");

        Id = id;
        JobId = jobId;
        ProviderId = providerId;
        StartUtc = startUtc;
        EndUtc = endUtc;
        Status = BookingStatus.Pending;
    }

    public Guid Id { get; }
    public Guid JobId { get; }
    public Guid ProviderId { get; }
    public DateTime StartUtc { get; }
    public DateTime EndUtc { get; }
    public BookingStatus Status { get; private set; }

    public void Confirm()
    {
        EnsureStatus(BookingStatus.Pending, "Only pending bookings can be confirmed.");
        Status = BookingStatus.Confirmed;
    }

    public void Decline()
    {
        EnsureStatus(BookingStatus.Pending, "Only pending bookings can be declined.");
        Status = BookingStatus.Declined;
    }

    public void Expire()
    {
        EnsureStatus(BookingStatus.Pending, "Only pending bookings can expire.");
        Status = BookingStatus.Expired;
    }

    public void Cancel()
    {
        EnsureStatus(BookingStatus.Confirmed, "Only confirmed bookings can be cancelled.");
        Status = BookingStatus.Cancelled;
    }

    private void EnsureStatus(BookingStatus expected, string message)
    {
        if (Status != expected) throw new DomainRuleViolationException(message);
    }
}

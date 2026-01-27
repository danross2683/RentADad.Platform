using System;

namespace RentADad.Application.Bookings.Requests;

public sealed record CreateBookingRequest(Guid JobId, Guid ProviderId, DateTime StartUtc, DateTime EndUtc);

using System;

namespace RentADad.Application.Bookings.Responses;

public sealed record BookingResponse(
    Guid Id,
    Guid JobId,
    Guid ProviderId,
    DateTime StartUtc,
    DateTime EndUtc,
    string Status,
    DateTime UpdatedUtc);

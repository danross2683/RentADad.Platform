using System;

namespace RentADad.Application.Jobs.Responses;

public sealed record JobResponse(
    Guid Id,
    Guid CustomerId,
    string Location,
    Guid[] ServiceIds,
    string Status,
    Guid? ActiveBookingId);

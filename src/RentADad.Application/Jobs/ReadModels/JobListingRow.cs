using System;

namespace RentADad.Application.Jobs.ReadModels;

public sealed record JobListingRow(
    Guid Id,
    Guid CustomerId,
    string Location,
    Guid[] ServiceIds,
    string Status,
    Guid? ActiveBookingId,
    DateTime UpdatedUtc);

using System;

namespace RentADad.Application.Jobs.ReadModels;

public sealed record JobListingWriteModel(
    Guid Id,
    Guid CustomerId,
    string Location,
    Guid[] ServiceIds,
    string Status,
    Guid? ActiveBookingId,
    DateTime UpdatedUtc);

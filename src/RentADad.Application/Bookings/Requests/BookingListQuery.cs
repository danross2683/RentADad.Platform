using System;
using RentADad.Domain.Bookings;

namespace RentADad.Application.Bookings.Requests;

public sealed record BookingListQuery(
    int Page,
    int PageSize,
    Guid? JobId,
    Guid? ProviderId,
    BookingStatus? Status,
    DateTime? StartUtcFrom,
    DateTime? StartUtcTo);

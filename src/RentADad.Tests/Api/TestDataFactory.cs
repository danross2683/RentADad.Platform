using System;
using System.Collections.Generic;
using RentADad.Application.Bookings.Requests;
using RentADad.Application.Jobs.Requests;
using RentADad.Application.Providers.Requests;

namespace RentADad.Tests.Api;

public static class TestDataFactory
{
    public static RegisterProviderRequest Provider(string? displayName = null)
    {
        return new RegisterProviderRequest(null, displayName ?? "Test Provider");
    }

    public static CreateJobRequest Job(Guid? customerId = null, string? location = null, List<Guid>? serviceIds = null)
    {
        return new CreateJobRequest(
            customerId ?? Guid.NewGuid(),
            location ?? "Test Location",
            serviceIds ?? new List<Guid> { Guid.NewGuid() });
    }

    public static CreateBookingRequest Booking(Guid jobId, Guid providerId, DateTime? startUtc = null, DateTime? endUtc = null)
    {
        var start = startUtc ?? DateTime.UtcNow.AddHours(1);
        var end = endUtc ?? start.AddHours(1);
        return new CreateBookingRequest(jobId, providerId, start, end);
    }
}

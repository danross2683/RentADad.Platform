using System;

namespace RentADad.Infrastructure.Persistence;

public sealed class JobListing
{
    private JobListing()
    {
        Location = string.Empty;
        ServiceIds = Array.Empty<Guid>();
        Status = string.Empty;
    }

    public JobListing(
        Guid id,
        Guid customerId,
        string location,
        Guid[] serviceIds,
        string status,
        Guid? activeBookingId,
        DateTime updatedUtc)
    {
        Id = id;
        CustomerId = customerId;
        Location = location;
        ServiceIds = serviceIds;
        Status = status;
        ActiveBookingId = activeBookingId;
        UpdatedUtc = updatedUtc;
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Location { get; private set; }
    public Guid[] ServiceIds { get; private set; }
    public string Status { get; private set; }
    public Guid? ActiveBookingId { get; private set; }
    public DateTime UpdatedUtc { get; private set; }

    public void UpdateFrom(JobListing source)
    {
        CustomerId = source.CustomerId;
        Location = source.Location;
        ServiceIds = source.ServiceIds;
        Status = source.Status;
        ActiveBookingId = source.ActiveBookingId;
        UpdatedUtc = source.UpdatedUtc;
    }
}

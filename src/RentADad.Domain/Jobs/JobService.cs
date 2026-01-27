using System;
using RentADad.Domain.Common;

namespace RentADad.Domain.Jobs;

public sealed class JobService
{
    private JobService()
    {
    }

    public JobService(Guid id, Guid serviceId)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("Job service id is required.");
        if (serviceId == Guid.Empty) throw new DomainRuleViolationException("Service id is required.");

        Id = id;
        ServiceId = serviceId;
    }

    public Guid Id { get; private set; }
    public Guid ServiceId { get; private set; }
}

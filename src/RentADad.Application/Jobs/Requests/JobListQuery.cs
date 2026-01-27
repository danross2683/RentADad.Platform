using System;
using RentADad.Domain.Jobs;

namespace RentADad.Application.Jobs.Requests;

public sealed record JobListQuery(
    int Page,
    int PageSize,
    Guid? CustomerId,
    JobStatus? Status);

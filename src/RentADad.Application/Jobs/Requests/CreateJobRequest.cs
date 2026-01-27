using System;
using System.Collections.Generic;

namespace RentADad.Application.Jobs.Requests;

public sealed record CreateJobRequest(Guid CustomerId, string? Location, List<Guid>? ServiceIds);

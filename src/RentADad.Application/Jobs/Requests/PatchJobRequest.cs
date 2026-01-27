using System;
using System.Collections.Generic;

namespace RentADad.Application.Jobs.Requests;

public sealed record PatchJobRequest(string? Location, List<Guid>? ServiceIds);

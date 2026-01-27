using System;
using System.Collections.Generic;

namespace RentADad.Application.Providers.Responses;

public sealed record ProviderResponse(
    Guid Id,
    string DisplayName,
    List<ProviderAvailabilityResponse> Availabilities,
    DateTime UpdatedUtc);

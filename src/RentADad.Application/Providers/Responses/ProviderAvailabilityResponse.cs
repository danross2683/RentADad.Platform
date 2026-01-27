using System;

namespace RentADad.Application.Providers.Responses;

public sealed record ProviderAvailabilityResponse(Guid Id, DateTime StartUtc, DateTime EndUtc);

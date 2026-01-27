using System;

namespace RentADad.Application.Providers.Requests;

public sealed record RegisterProviderRequest(Guid? ProviderId, string? DisplayName);

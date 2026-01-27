using System;

namespace RentADad.Application.Providers.Requests;

public sealed record AddAvailabilityRequest(DateTime StartUtc, DateTime EndUtc);

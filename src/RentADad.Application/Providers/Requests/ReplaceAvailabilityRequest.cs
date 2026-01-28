using System;
using System.Collections.Generic;

namespace RentADad.Application.Providers.Requests;

public sealed record AvailabilitySlot(DateTime StartUtc, DateTime EndUtc);

public sealed record ReplaceAvailabilityRequest(List<AvailabilitySlot> Slots);

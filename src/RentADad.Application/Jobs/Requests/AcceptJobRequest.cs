using System;

namespace RentADad.Application.Jobs.Requests;

public sealed record AcceptJobRequest(Guid BookingId);

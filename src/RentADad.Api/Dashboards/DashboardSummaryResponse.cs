using System;
using System.Collections.Generic;
using RentADad.Application.Abstractions.Observability;

namespace RentADad.Api.Dashboards;

public sealed record DashboardSummaryResponse(
    string Version,
    DateTime GeneratedUtc,
    long UptimeSeconds,
    IReadOnlyDictionary<string, int> JobsByStatus,
    IReadOnlyDictionary<string, int> BookingsByStatus,
    int TotalProviders,
    AlertingThresholds Alerting);

namespace RentADad.Application.Abstractions.Observability;

public sealed class AlertingThresholds
{
    public double ErrorRate5xxPercent { get; set; } = 1.0;
    public int LatencyP95Ms { get; set; } = 1500;
    public int LatencyP99Ms { get; set; } = 3000;
    public double AuthFailurePercent { get; set; } = 5.0;
    public int DbP95Ms { get; set; } = 500;
    public int BackgroundJobStaleMinutes { get; set; } = 10;
}

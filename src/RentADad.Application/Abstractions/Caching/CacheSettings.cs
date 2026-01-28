namespace RentADad.Application.Abstractions.Caching;

public sealed class CacheSettings
{
    public int ProviderAvailabilitySeconds { get; set; } = 30;
}

namespace RentADad.Application.Providers.Requests;

public sealed record ProviderListQuery(
    int Page,
    int PageSize,
    string? DisplayNameContains);

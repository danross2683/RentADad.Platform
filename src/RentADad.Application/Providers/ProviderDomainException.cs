using System;

namespace RentADad.Application.Providers;

public sealed class ProviderDomainException : Exception
{
    public ProviderDomainException(string message)
        : base(message)
    {
    }
}

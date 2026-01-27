using System;

namespace RentADad.Application.Providers;

public sealed class ProviderDomainException : Exception
{
    public ProviderDomainException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}

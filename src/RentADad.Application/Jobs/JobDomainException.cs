using System;

namespace RentADad.Application.Jobs;

public sealed class JobDomainException : Exception
{
    public JobDomainException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}

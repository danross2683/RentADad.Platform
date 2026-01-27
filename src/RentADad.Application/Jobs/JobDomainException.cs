using System;

namespace RentADad.Application.Jobs;

public sealed class JobDomainException : Exception
{
    public JobDomainException(string message)
        : base(message)
    {
    }
}

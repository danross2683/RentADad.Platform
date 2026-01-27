using System;

namespace RentADad.Application.Bookings;

public sealed class BookingDomainException : Exception
{
    public BookingDomainException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}

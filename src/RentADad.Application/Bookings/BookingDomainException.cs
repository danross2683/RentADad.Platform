using System;

namespace RentADad.Application.Bookings;

public sealed class BookingDomainException : Exception
{
    public BookingDomainException(string message)
        : base(message)
    {
    }
}

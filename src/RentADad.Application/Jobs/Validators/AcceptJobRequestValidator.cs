using FluentValidation;
using RentADad.Application.Jobs.Requests;

namespace RentADad.Application.Jobs.Validators;

public sealed class AcceptJobRequestValidator : AbstractValidator<AcceptJobRequest>
{
    public AcceptJobRequestValidator()
    {
        RuleFor(x => x.BookingId).NotEmpty();
    }
}

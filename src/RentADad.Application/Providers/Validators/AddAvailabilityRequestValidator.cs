using FluentValidation;
using RentADad.Application.Providers.Requests;

namespace RentADad.Application.Providers.Validators;

public sealed class AddAvailabilityRequestValidator : AbstractValidator<AddAvailabilityRequest>
{
    public AddAvailabilityRequestValidator()
    {
        RuleFor(x => x.StartUtc).NotEmpty();
        RuleFor(x => x.EndUtc).NotEmpty().GreaterThan(x => x.StartUtc);
    }
}

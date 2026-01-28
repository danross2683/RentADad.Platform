using FluentValidation;
using RentADad.Application.Providers.Requests;

namespace RentADad.Application.Providers.Validators;

public sealed class ReplaceAvailabilityRequestValidator : AbstractValidator<ReplaceAvailabilityRequest>
{
    public ReplaceAvailabilityRequestValidator()
    {
        RuleFor(x => x.Slots).NotNull();
        RuleForEach(x => x.Slots).ChildRules(slot =>
        {
            slot.RuleFor(x => x.StartUtc).NotEmpty();
            slot.RuleFor(x => x.EndUtc).NotEmpty();
        });
    }
}

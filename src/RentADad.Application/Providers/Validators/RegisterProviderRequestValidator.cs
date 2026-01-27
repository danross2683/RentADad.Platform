using FluentValidation;
using RentADad.Application.Providers.Requests;

namespace RentADad.Application.Providers.Validators;

public sealed class RegisterProviderRequestValidator : AbstractValidator<RegisterProviderRequest>
{
    public RegisterProviderRequestValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
    }
}

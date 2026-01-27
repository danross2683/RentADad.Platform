using FluentValidation;
using RentADad.Application.Providers.Requests;

namespace RentADad.Application.Providers.Validators;

public sealed class UpdateProviderRequestValidator : AbstractValidator<UpdateProviderRequest>
{
    public UpdateProviderRequestValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
    }
}

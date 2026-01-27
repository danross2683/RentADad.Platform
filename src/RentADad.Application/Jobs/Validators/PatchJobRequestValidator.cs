using FluentValidation;
using RentADad.Application.Jobs.Requests;

namespace RentADad.Application.Jobs.Validators;

public sealed class PatchJobRequestValidator : AbstractValidator<PatchJobRequest>
{
    public PatchJobRequestValidator()
    {
        RuleFor(x => x.Location).MaximumLength(256);
        RuleForEach(x => x.ServiceIds).NotEmpty();
    }
}

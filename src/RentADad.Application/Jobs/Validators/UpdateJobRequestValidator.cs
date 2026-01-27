using FluentValidation;
using RentADad.Application.Jobs.Requests;

namespace RentADad.Application.Jobs.Validators;

public sealed class UpdateJobRequestValidator : AbstractValidator<UpdateJobRequest>
{
    public UpdateJobRequestValidator()
    {
        RuleFor(x => x.Location).NotEmpty().MaximumLength(256);
        RuleFor(x => x.ServiceIds).NotNull();
        RuleForEach(x => x.ServiceIds).NotEmpty();
    }
}

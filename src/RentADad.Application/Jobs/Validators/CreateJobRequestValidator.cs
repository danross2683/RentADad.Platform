using FluentValidation;
using RentADad.Application.Jobs.Requests;

namespace RentADad.Application.Jobs.Validators;

public sealed class CreateJobRequestValidator : AbstractValidator<CreateJobRequest>
{
    public CreateJobRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Location).NotEmpty().MaximumLength(256);
        RuleFor(x => x.ServiceIds).NotNull();
        RuleForEach(x => x.ServiceIds).NotEmpty();
    }
}

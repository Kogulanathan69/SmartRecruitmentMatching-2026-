using FluentValidation;
using NexHire.Application.DTOs.ConsentContact;

namespace NexHire.Application.Validators;

public class ContactDecisionRequestValidator : AbstractValidator<ContactDecisionRequestDto>
{
    public ContactDecisionRequestValidator()
    {
        RuleFor(x => x.Decision)
            .NotEmpty()
            .Must(value => value.Equals("Accept", StringComparison.OrdinalIgnoreCase) ||
                           value.Equals("Decline", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Decision must be Accept or Decline.");
    }
}

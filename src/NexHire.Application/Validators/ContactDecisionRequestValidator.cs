using FluentValidation;
using NexHire.Application.DTOs.ConsentContact;

namespace NexHire.Application.Validators;

public class ContactDecisionRequestValidator
    : AbstractValidator<ContactDecisionRequestDto>
{
    private static readonly string[] Allowed =
    [
        "Accept",
        "Accepted",
        "Decline",
        "Declined"
    ];

    public ContactDecisionRequestValidator()
    {
        RuleFor(x => x.Decision)
            .NotEmpty()
            .Must(value =>
                Allowed.Any(allowed =>
                    allowed.Equals(
                        value,
                        StringComparison.OrdinalIgnoreCase)))
            .WithMessage(
                "Decision must be Accept, Accepted, Decline or Declined.");
    }
}

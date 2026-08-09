using FluentValidation;
using NexHire.Application.DTOs.ApplicationStatus;

namespace NexHire.Application.Validators;

public class UpdateApplicationStatusRequestValidator
    : AbstractValidator<UpdateApplicationStatusRequestDto>
{
    private static readonly string[] Allowed =
    [
        "UnderReview",
        "Shortlisted",
        "WaitingList",
        "Rejected"
    ];

    public UpdateApplicationStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(value =>
                Allowed.Any(allowed =>
                    allowed.Equals(
                        value,
                        StringComparison.OrdinalIgnoreCase)))
            .WithMessage(
                "Status must be UnderReview, Shortlisted, WaitingList or Rejected.");

        RuleFor(x => x.Reason)
            .MaximumLength(500);
    }
}

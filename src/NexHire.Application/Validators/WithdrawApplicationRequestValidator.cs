using FluentValidation;
using NexHire.Application.DTOs.ApplicationStatus;

namespace NexHire.Application.Validators;

public class WithdrawApplicationRequestValidator : AbstractValidator<WithdrawApplicationRequestDto>
{
    public WithdrawApplicationRequestValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

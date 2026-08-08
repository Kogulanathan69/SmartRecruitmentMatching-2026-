using FluentValidation;
using NexHire.Application.DTOs.Admin;
namespace NexHire.Application.Validators;
public sealed class AdminResetPasswordRequestValidator : AbstractValidator<AdminResetPasswordRequestDto>
{
    public AdminResetPasswordRequestValidator()
    {
        RuleFor(x => x.NewPassword)
            .Must(PasswordPolicy.IsStrong)
            .WithMessage("Password must contain uppercase, lowercase, number and symbol.");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword);
    }
}

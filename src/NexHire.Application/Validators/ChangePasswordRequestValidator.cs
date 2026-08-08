using FluentValidation;
using NexHire.Application.DTOs.Auth;

namespace NexHire.Application.Validators;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(request => request.CurrentPassword)
            .NotEmpty();

        RuleFor(request => request.NewPassword)
            .Must(PasswordPolicy.IsStrong)
            .WithMessage(PasswordPolicy.ErrorMessage)
            .NotEqual(request => request.CurrentPassword)
            .WithMessage("The new password must differ from the current password.");

        RuleFor(request => request.ConfirmNewPassword)
            .Equal(request => request.NewPassword)
            .WithMessage("Password confirmation does not match.");
    }
}

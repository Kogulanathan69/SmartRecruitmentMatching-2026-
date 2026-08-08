using FluentValidation;
using NexHire.Application.DTOs.Auth;

namespace NexHire.Application.Validators;

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(request => request.Token)
            .NotEmpty();

        RuleFor(request => request.NewPassword)
            .Must(PasswordPolicy.IsStrong)
            .WithMessage(PasswordPolicy.ErrorMessage);

        RuleFor(request => request.ConfirmNewPassword)
            .Equal(request => request.NewPassword)
            .WithMessage("Password confirmation does not match.");
    }
}

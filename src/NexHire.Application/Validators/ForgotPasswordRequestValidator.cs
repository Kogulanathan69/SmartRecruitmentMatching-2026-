using FluentValidation;
using NexHire.Application.DTOs.Auth;

namespace NexHire.Application.Validators;

public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDto>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}

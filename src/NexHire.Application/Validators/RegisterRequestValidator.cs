using FluentValidation;
using NexHire.Application.DTOs.Auth;

namespace NexHire.Application.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.FirstName)
            .NotEmpty()
            .MaximumLength(60);

        RuleFor(request => request.LastName)
            .NotEmpty()
            .MaximumLength(60);

        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(request => request.Password)
            .Must(PasswordPolicy.IsStrong)
            .WithMessage(PasswordPolicy.ErrorMessage);

        RuleFor(request => request.ConfirmPassword)
            .Equal(request => request.Password)
            .WithMessage("Password confirmation does not match.");

        RuleFor(request => request.Role)
            .Must(IsPublicRegistrationRole)
            .WithMessage("Role must be JobSeeker or Employer.");
    }

    private static bool IsPublicRegistrationRole(string role)
    {
        return role.Equals("JobSeeker", StringComparison.OrdinalIgnoreCase)
            || role.Equals("Employer", StringComparison.OrdinalIgnoreCase);
    }
}

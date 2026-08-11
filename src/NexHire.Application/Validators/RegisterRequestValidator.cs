using FluentValidation; using NexHire.Application.DTOs.Auth;
namespace NexHire.Application.Validators;
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator(){RuleFor(x=>x.FirstName).NotEmpty().MaximumLength(60);RuleFor(x=>x.LastName).NotEmpty().MaximumLength(60);RuleFor(x=>x.Email).NotEmpty().EmailAddress().MaximumLength(256);RuleFor(x=>x.Password).Must(PasswordPolicy.IsStrong).WithMessage("Password must be 8-64 characters and contain uppercase, lowercase, number and symbol.");RuleFor(x=>x.ConfirmPassword).Equal(x=>x.Password);RuleFor(x=>x.Role).Must(x=>x.Equals("JobSeeker",StringComparison.OrdinalIgnoreCase)||x.Equals("Employer",StringComparison.OrdinalIgnoreCase)).WithMessage("Role must be JobSeeker or Employer.");}
}

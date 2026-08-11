using FluentValidation;
using NexHire.Application.DTOs.Admin;
using NexHire.Domain.Enums;
namespace NexHire.Application.Validators;
public sealed class CreateMemberRequestValidator : AbstractValidator<CreateMemberRequestDto>
{
    public CreateMemberRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(60);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password)
            .Must(PasswordPolicy.IsStrong)
            .WithMessage("Password must be 8-64 characters with uppercase, lowercase, number and symbol.");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password);
        RuleFor(x => x.Role).Must(x => Enum.TryParse<UserRole>(x, true, out _));
    }
}

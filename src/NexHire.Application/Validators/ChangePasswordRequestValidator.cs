using FluentValidation; using NexHire.Application.DTOs.Auth;
namespace NexHire.Application.Validators;
public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequestDto>{public ChangePasswordRequestValidator(){RuleFor(x=>x.CurrentPassword).NotEmpty();RuleFor(x=>x.NewPassword).Must(PasswordPolicy.IsStrong).WithMessage("New password must contain uppercase, lowercase, number and symbol.").NotEqual(x=>x.CurrentPassword);RuleFor(x=>x.ConfirmNewPassword).Equal(x=>x.NewPassword);}}

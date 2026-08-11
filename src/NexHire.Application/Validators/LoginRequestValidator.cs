using FluentValidation; using NexHire.Application.DTOs.Auth;
namespace NexHire.Application.Validators;
public sealed class LoginRequestValidator : AbstractValidator<LoginRequestDto>{public LoginRequestValidator(){RuleFor(x=>x.Email).NotEmpty().EmailAddress();RuleFor(x=>x.Password).NotEmpty();}}

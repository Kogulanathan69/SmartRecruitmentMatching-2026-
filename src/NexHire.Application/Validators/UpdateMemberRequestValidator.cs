using FluentValidation;
using NexHire.Application.DTOs.Admin;
using NexHire.Domain.Enums;
namespace NexHire.Application.Validators;
public sealed class UpdateMemberRequestValidator : AbstractValidator<UpdateMemberRequestDto>
{
    public UpdateMemberRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(60);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Role).Must(x => Enum.TryParse<UserRole>(x, true, out _));
    }
}

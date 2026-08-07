using FluentValidation;
using NexHire.Application.DTOs.ConsentContact;

namespace NexHire.Application.Validators;

public class CreateContactRequestValidator : AbstractValidator<CreateContactRequestDto>
{
    public CreateContactRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(1000);
    }
}

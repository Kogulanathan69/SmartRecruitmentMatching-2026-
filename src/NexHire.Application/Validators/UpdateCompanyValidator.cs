using FluentValidation;
using NexHire.Application.DTOs.Company;

namespace NexHire.Application.Validators;

public sealed class UpdateCompanyValidator
    : AbstractValidator<UpdateCompanyDto>
{
    public UpdateCompanyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.Name is not null);

        RuleFor(x => x.LegalName)
            .MaximumLength(250)
            .When(x => x.LegalName is not null);

        RuleFor(x => x.OfficialEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(250)
            .When(x => x.OfficialEmail is not null);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(30)
            .When(x => x.PhoneNumber is not null);

        RuleFor(x => x.RegisteredAddress)
            .NotEmpty()
            .MaximumLength(500)
            .When(x => x.RegisteredAddress is not null);

        RuleFor(x => x.Website)
            .MaximumLength(300)
            .When(x => x.Website is not null);
    }
}
using FluentValidation;
using NexHire.Application.DTOs.JobSeeker;

namespace NexHire.Application.Validators.JobSeeker;

public class AddExperienceValidator : AbstractValidator<AddExperienceDto>
{
    public AddExperienceValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.JobTitle).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x)
            .Must(x => x.IsCurrent || !x.EndDate.HasValue || x.EndDate.Value.Date >= x.StartDate.Date)
            .WithMessage("EndDate cannot be earlier than StartDate.");
        RuleFor(x => x.EndDate)
            .Null()
            .When(x => x.IsCurrent)
            .WithMessage("Current experience must not have an EndDate.");
    }
}

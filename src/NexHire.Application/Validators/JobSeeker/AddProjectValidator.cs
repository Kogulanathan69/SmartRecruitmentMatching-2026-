using FluentValidation;
using NexHire.Application.DTOs.JobSeeker;

namespace NexHire.Application.Validators.JobSeeker;

public class AddProjectValidator : AbstractValidator<AddProjectDto>
{
    public AddProjectValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.TechStack).MaximumLength(500);
        RuleFor(x => x.ProjectUrl).MaximumLength(500);
        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.EndDate.Value.Date >= x.StartDate.Value.Date)
            .WithMessage("EndDate cannot be earlier than StartDate.");
    }
}

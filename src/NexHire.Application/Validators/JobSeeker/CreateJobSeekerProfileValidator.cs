using FluentValidation;
using NexHire.Application.DTOs.JobSeeker;

namespace NexHire.Application.Validators.JobSeeker;

public class CreateJobSeekerProfileValidator : AbstractValidator<CreateJobSeekerProfileDto>
{
    public CreateJobSeekerProfileValidator()
    {
        RuleFor(x => x.Headline).MaximumLength(200);
        RuleFor(x => x.Summary).MaximumLength(2000);
        RuleFor(x => x.Gender).MaximumLength(50);
        RuleFor(x => x.Address).MaximumLength(300);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.Country).MaximumLength(100);
        RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 60);
        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .When(x => x.DateOfBirth.HasValue);
        RuleFor(x => x.ExpectedSalaryMin)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ExpectedSalaryMin.HasValue);
        RuleFor(x => x.ExpectedSalaryMax)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ExpectedSalaryMax.HasValue);
        RuleFor(x => x)
            .Must(x => !x.ExpectedSalaryMin.HasValue || !x.ExpectedSalaryMax.HasValue || x.ExpectedSalaryMin <= x.ExpectedSalaryMax)
            .WithMessage("ExpectedSalaryMin cannot be greater than ExpectedSalaryMax.");
    }
}

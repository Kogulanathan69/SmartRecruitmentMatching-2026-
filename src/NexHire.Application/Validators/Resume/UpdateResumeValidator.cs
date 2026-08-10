using FluentValidation;
using NexHire.Application.DTOs.Resume;

namespace NexHire.Application.Validators.Resume;

public class UpdateResumeValidator : AbstractValidator<UpdateResumeDto>
{
    public UpdateResumeValidator()
    {
        RuleFor(x => x.ResumeName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.CareerObjective).MaximumLength(1200);
        RuleFor(x => x.Languages)
            .Must(x => x.Count(v => !string.IsNullOrWhiteSpace(v)) <= 10)
            .WithMessage("A maximum of 10 languages is allowed.");
        RuleFor(x => x.LinkedInUrl).MaximumLength(500);
        RuleFor(x => x.GitHubUrl).MaximumLength(500);
        RuleFor(x => x.PortfolioUrl).MaximumLength(500);
    }
}

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
            .NotNull()
            .Must(HaveAtMostTenLanguages)
            .WithMessage("A maximum of 10 languages is allowed.")
            .Must(FitStoredLanguageLength)
            .WithMessage("Languages cannot exceed 500 characters in total.");
        RuleForEach(x => x.Languages)
            .MaximumLength(80)
            .When(x => x.Languages is not null);
        RuleFor(x => x.LinkedInUrl).MaximumLength(500).Must(BeWebUrl).WithMessage("LinkedIn URL must be an absolute HTTP or HTTPS URL.");
        RuleFor(x => x.GitHubUrl).MaximumLength(500).Must(BeWebUrl).WithMessage("GitHub URL must be an absolute HTTP or HTTPS URL.");
        RuleFor(x => x.PortfolioUrl).MaximumLength(500).Must(BeWebUrl).WithMessage("Portfolio URL must be an absolute HTTP or HTTPS URL.");
    }

    private static bool HaveAtMostTenLanguages(List<string>? values) =>
        values is not null &&
        values.Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() <= 10;

    private static bool FitStoredLanguageLength(List<string>? values) =>
        values is not null &&
        string.Join(",", values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)).Length <= 500;

    private static bool BeWebUrl(string? value) =>
        string.IsNullOrWhiteSpace(value) ||
        Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

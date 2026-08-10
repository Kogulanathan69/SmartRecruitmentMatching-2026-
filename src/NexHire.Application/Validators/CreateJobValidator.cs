using FluentValidation;
using NexHire.Application.DTOs.Job;

namespace NexHire.Application.Validators;

public class CreateJobValidator : AbstractValidator<CreateJobDto>
{
    public CreateJobValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(5000);

        RuleFor(x => x)
            .Must(x =>
                !x.SalaryMin.HasValue ||
                !x.SalaryMax.HasValue ||
                x.SalaryMin <= x.SalaryMax)
            .WithMessage(
                "Salary minimum cannot exceed salary maximum.");
    }
}
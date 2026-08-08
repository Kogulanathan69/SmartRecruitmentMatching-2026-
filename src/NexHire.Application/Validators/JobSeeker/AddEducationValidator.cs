using FluentValidation;
using NexHire.Application.DTOs.JobSeeker;

namespace NexHire.Application.Validators.JobSeeker;

public class AddEducationValidator : AbstractValidator<AddEducationDto>
{
    public AddEducationValidator()
    {
        RuleFor(x => x.Institution).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Degree).NotEmpty().MaximumLength(150);
        RuleFor(x => x.FieldOfStudy).MaximumLength(150);
        RuleFor(x => x.GradeOrGpa).MaximumLength(50);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x)
            .Must(x => !x.EndDate.HasValue || x.EndDate.Value.Date >= x.StartDate.Date)
            .WithMessage("EndDate cannot be earlier than StartDate.");
    }
}

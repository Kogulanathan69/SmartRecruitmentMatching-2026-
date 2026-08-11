using FluentValidation;
using NexHire.Application.DTOs.JobSeeker;

namespace NexHire.Application.Validators.JobSeeker;

public class AddSkillValidator : AbstractValidator<AddSkillDto>
{
    public AddSkillValidator()
    {
        RuleFor(x => x.SkillName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ProficiencyLevel).InclusiveBetween(1, 5);
        RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 60);
    }
}

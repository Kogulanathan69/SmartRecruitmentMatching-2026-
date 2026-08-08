using Xunit;
using FluentAssertions;
using NexHire.Application.DTOs.JobSeeker;
using NexHire.Application.Validators.JobSeeker;

namespace NexHire.UnitTests.JobSeeker;

public class JobSeekerValidationTests
{
    [Fact]
    public void CreateProfile_MinSalaryGreaterThanMaxSalary_IsInvalid()
    {
        var validator = new CreateJobSeekerProfileValidator();
        var dto = new CreateJobSeekerProfileDto
        {
            YearsOfExperience = 2,
            ExpectedSalaryMin = 200000,
            ExpectedSalaryMax = 100000
        };

        var result = validator.Validate(dto);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Education_EndBeforeStart_IsInvalid()
    {
        var validator = new AddEducationValidator();
        var dto = new AddEducationDto
        {
            Institution = "Example Institute",
            Degree = "BSc",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2024, 1, 1)
        };

        validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Skill_ProficiencyOutsideOneToFive_IsInvalid()
    {
        var validator = new AddSkillValidator();
        var dto = new AddSkillDto
        {
            SkillName = "C#",
            ProficiencyLevel = 6,
            YearsOfExperience = 1
        };

        validator.Validate(dto).IsValid.Should().BeFalse();
    }
}

using FluentAssertions;
using NexHire.Application.DTOs.Resume;
using NexHire.Application.Validators.Resume;

namespace NexHire.UnitTests.JobSeeker;

public class ResumeValidationTests
{
    [Fact]
    public void CreateResume_EmptyName_IsInvalid()
    {
        var validator = new CreateResumeValidator();
        var dto = new CreateResumeDto { ResumeName = string.Empty };

        validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateResume_MoreThanTenLanguages_IsInvalid()
    {
        var validator = new CreateResumeValidator();
        var dto = new CreateResumeDto
        {
            ResumeName = "Developer CV",
            Languages = Enumerable.Range(1, 11).Select(x => $"Language{x}").ToList()
        };

        validator.Validate(dto).IsValid.Should().BeFalse();
    }
}

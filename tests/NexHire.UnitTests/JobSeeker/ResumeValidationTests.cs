using Xunit;
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

    [Fact]
    public void CreateResume_NullLanguages_IsInvalidWithoutThrowing()
    {
        var validator = new CreateResumeValidator();
        var dto = new CreateResumeDto
        {
            ResumeName = "Developer CV",
            Languages = null!
        };

        var result = validator.Validate(dto);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateResume_LanguagesAboveStoredLength_IsInvalid()
    {
        var validator = new CreateResumeValidator();
        var dto = new CreateResumeDto
        {
            ResumeName = "Developer CV",
            Languages = Enumerable.Range(1, 8)
                .Select(index => $"Language-{index}-{new string('x', 60)}")
                .ToList()
        };

        validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("linkedin.com/in/candidate")]
    public void UpdateResume_NonHttpLink_IsInvalid(string link)
    {
        var validator = new UpdateResumeValidator();
        var dto = new UpdateResumeDto
        {
            ResumeName = "Developer CV",
            LinkedInUrl = link
        };

        validator.Validate(dto).IsValid.Should().BeFalse();
    }
}

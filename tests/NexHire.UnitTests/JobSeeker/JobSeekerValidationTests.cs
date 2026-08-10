using Xunit;
using FluentAssertions;
using AutoMapper;
using Moq;
using NexHire.Application.Common.Exceptions;
using NexHire.Application.DTOs.JobSeeker;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Services;
using NexHire.Application.Validators.JobSeeker;
using NexHire.Domain.Entities;

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
            EducationLevel = 3,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2024, 1, 1)
        };

        validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Education_LevelOutsideSupportedScale_IsInvalid()
    {
        var validator = new AddEducationValidator();
        var dto = new AddEducationDto
        {
            Institution = "Example Institute",
            Degree = "BSc",
            EducationLevel = 7,
            StartDate = new DateTime(2025, 1, 1)
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

    [Fact]
    public void UpdateProfile_ClearFlagAndReplacementValue_IsInvalid()
    {
        var validator = new UpdateJobSeekerProfileValidator();
        var dto = new UpdateJobSeekerProfileDto
        {
            DateOfBirth = new DateTime(2000, 1, 1),
            ClearDateOfBirth = true
        };

        validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateProfile_ClearFlags_RemoveNullableValues()
    {
        var userId = Guid.NewGuid();
        var profile = new JobSeekerProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DateOfBirth = new DateTime(2000, 1, 1),
            ExpectedSalaryMin = 100000,
            ExpectedSalaryMax = 200000,
            YearsOfExperience = 2
        };

        var repository = new Mock<IJobSeekerRepository>();
        repository
            .Setup(x => x.GetByUserIdWithDetailsAsync(userId))
            .ReturnsAsync(profile);
        repository
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var mapper = new Mock<IMapper>();
        mapper
            .Setup(x => x.Map(It.IsAny<UpdateJobSeekerProfileDto>(), profile))
            .Returns(profile);
        mapper
            .Setup(x => x.Map<JobSeekerProfileResponseDto>(profile))
            .Returns(new JobSeekerProfileResponseDto());
        var service = new JobSeekerService(
            repository.Object,
            mapper.Object);

        await service.UpdateProfileAsync(
            userId,
            new UpdateJobSeekerProfileDto
            {
                ClearDateOfBirth = true,
                ClearExpectedSalaryMin = true,
                ClearExpectedSalaryMax = true
            });

        profile.DateOfBirth.Should().BeNull();
        profile.ExpectedSalaryMin.Should().BeNull();
        profile.ExpectedSalaryMax.Should().BeNull();
        repository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddEvidence_UsesExplicitRepositoryAddsForEveryNewEntity()
    {
        var userId = Guid.NewGuid();
        var profile = new JobSeekerProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId
        };
        var skill = new Skill { Id = Guid.NewGuid(), Name = "C#" };

        var repository = new Mock<IJobSeekerRepository>();
        repository
            .Setup(x => x.GetByUserIdWithDetailsAsync(userId))
            .ReturnsAsync(profile);
        repository
            .Setup(x => x.GetSkillByNameAsync("C#"))
            .ReturnsAsync(skill);
        repository.Setup(x => x.AddEducationAsync(It.IsAny<Education>())).Returns(Task.CompletedTask);
        repository.Setup(x => x.AddExperienceAsync(It.IsAny<Experience>())).Returns(Task.CompletedTask);
        repository.Setup(x => x.AddCandidateSkillAsync(It.IsAny<CandidateSkill>())).Returns(Task.CompletedTask);
        repository.Setup(x => x.AddProjectAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);
        repository.Setup(x => x.AddCertificationAsync(It.IsAny<Certification>())).Returns(Task.CompletedTask);
        repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var educationDto = new AddEducationDto
        {
            Institution = "Example Institute",
            Degree = "BSc",
            EducationLevel = 3,
            StartDate = new DateTime(2022, 1, 1)
        };
        var experienceDto = new AddExperienceDto
        {
            CompanyName = "NexHire Labs",
            JobTitle = "Developer",
            StartDate = new DateTime(2025, 1, 1),
            IsCurrent = true
        };
        var skillDto = new AddSkillDto
        {
            SkillName = "C#",
            ProficiencyLevel = 4,
            YearsOfExperience = 2
        };
        var projectDto = new AddProjectDto { Title = "Recruitment platform" };
        var certificationDto = new AddCertificationDto { Name = "Azure Fundamentals" };

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<Education>(educationDto)).Returns(new Education());
        mapper.Setup(x => x.Map<Experience>(experienceDto)).Returns(new Experience());
        mapper.Setup(x => x.Map<Project>(projectDto)).Returns(new Project());
        mapper.Setup(x => x.Map<Certification>(certificationDto)).Returns(new Certification());
        mapper.Setup(x => x.Map<EducationResponseDto>(It.IsAny<Education>())).Returns(new EducationResponseDto());
        mapper.Setup(x => x.Map<ExperienceResponseDto>(It.IsAny<Experience>())).Returns(new ExperienceResponseDto());
        mapper.Setup(x => x.Map<CandidateSkillResponseDto>(It.IsAny<CandidateSkill>())).Returns(new CandidateSkillResponseDto());
        mapper.Setup(x => x.Map<ProjectResponseDto>(It.IsAny<Project>())).Returns(new ProjectResponseDto());
        mapper.Setup(x => x.Map<CertificationResponseDto>(It.IsAny<Certification>())).Returns(new CertificationResponseDto());

        var service = new JobSeekerService(repository.Object, mapper.Object);

        await service.AddEducationAsync(userId, educationDto);
        await service.AddExperienceAsync(userId, experienceDto);
        await service.AddSkillAsync(userId, skillDto);
        await service.AddProjectAsync(userId, projectDto);
        await service.AddCertificationAsync(userId, certificationDto);

        repository.Verify(x => x.AddEducationAsync(It.IsAny<Education>()), Times.Once);
        repository.Verify(x => x.AddExperienceAsync(It.IsAny<Experience>()), Times.Once);
        repository.Verify(x => x.AddCandidateSkillAsync(It.IsAny<CandidateSkill>()), Times.Once);
        repository.Verify(x => x.AddProjectAsync(It.IsAny<Project>()), Times.Once);
        repository.Verify(x => x.AddCertificationAsync(It.IsAny<Certification>()), Times.Once);
        repository.Verify(x => x.SaveChangesAsync(), Times.Exactly(5));
    }

    [Fact]
    public async Task AddEducation_InvalidLevel_StopsBeforePersistenceWithValidationError()
    {
        var repository = new Mock<IJobSeekerRepository>();
        var service = new JobSeekerService(
            repository.Object,
            Mock.Of<IMapper>());

        var action = () => service.AddEducationAsync(
            Guid.NewGuid(),
            new AddEducationDto
            {
                Institution = "Example Institute",
                Degree = "BSc",
                EducationLevel = 7,
                StartDate = new DateTime(2025, 1, 1)
            });

        await action.Should().ThrowAsync<ValidationException>();
        repository.Verify(
            x => x.GetByUserIdWithDetailsAsync(It.IsAny<Guid>()),
            Times.Never);
        repository.Verify(
            x => x.AddEducationAsync(It.IsAny<Education>()),
            Times.Never);
        repository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
}

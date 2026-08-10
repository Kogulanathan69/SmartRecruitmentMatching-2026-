using System.Text;
using FluentAssertions;
using NexHire.Application.Common.Exceptions;
using NexHire.Application.DTOs.Resume;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Services;
using NexHire.Domain.Entities;
using Xunit;

namespace NexHire.UnitTests.JobSeeker;

public class ResumeServiceTests
{
    [Fact]
    public async Task CreateBuilder_FirstResumeUsesSelectedTemplateAndBecomesPrimary()
    {
        var fixture = new ResumeFixture();
        var template = fixture.AddTemplate("modern", "Modern");

        var result = await fixture.Service.CreateAsync(
            fixture.UserId,
            fixture.ValidCreate(template.Id));

        result.IsPrimary.Should().BeTrue();
        result.TemplateId.Should().Be(template.Id);
        result.TemplateName.Should().Be("Modern");
        fixture.Profile.Resumes.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateBuilder_DuplicateNameIgnoringCaseReturnsBusinessConflict()
    {
        var fixture = new ResumeFixture();
        await fixture.Service.CreateAsync(fixture.UserId, fixture.ValidCreate());
        var duplicate = fixture.ValidCreate();
        duplicate.ResumeName = "  DEVELOPER CV  ";

        var action = () => fixture.Service.CreateAsync(fixture.UserId, duplicate);

        await action.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task CreateBuilder_InvalidLinkReturnsValidationErrorBeforePersistence()
    {
        var fixture = new ResumeFixture();
        var dto = fixture.ValidCreate();
        dto.LinkedInUrl = "javascript:alert(1)";

        var action = () => fixture.Service.CreateAsync(fixture.UserId, dto);

        await action.Should().ThrowAsync<ValidationException>();
        fixture.Profile.Resumes.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateHtml_UsesSelectedTemplateAndEncodesProfileContent()
    {
        var fixture = new ResumeFixture(completeProfile: true);
        var template = fixture.AddTemplate("technical", "Technical");
        var created = await fixture.Service.CreateAsync(
            fixture.UserId,
            fixture.ValidCreate(template.Id));
        fixture.Profile.Headline = "Developer <script>alert(1)</script>";

        var html = await fixture.Service.GenerateHtmlAsync(fixture.UserId, created.Id);

        html.Should().Contain("data-template='technical'");
        html.Should().Contain("Developer &lt;script&gt;alert(1)&lt;/script&gt;");
        html.Should().NotContain("Developer <script>");
        fixture.Profile.Resumes.Single().IsGenerated.Should().BeTrue();
    }

    [Fact]
    public async Task Upload_FirstValidPdfBecomesPrimaryAndCanBeDownloaded()
    {
        var fixture = new ResumeFixture();
        var content = Encoding.ASCII.GetBytes("%PDF-1.7 test CV");

        var uploaded = await fixture.Service.UploadAsync(
            fixture.UserId,
            "candidate.pdf",
            content,
            "Uploaded CV",
            false);
        var download = await fixture.Service.DownloadUploadedAsync(
            fixture.UserId,
            uploaded.Id);

        uploaded.IsPrimary.Should().BeTrue();
        uploaded.FileUrl.Should().EndWith("/file");
        download.Content.Should().Equal(content);
        download.ContentType.Should().Be("application/pdf");
        download.DownloadName.Should().Be("Uploaded CV.pdf");
    }

    [Fact]
    public async Task GenerateHtml_UploadedResumeReturnsBusinessConflict()
    {
        var fixture = new ResumeFixture(completeProfile: true);
        var uploaded = await fixture.Service.UploadAsync(
            fixture.UserId,
            "candidate.pdf",
            Encoding.ASCII.GetBytes("%PDF-1.7 test CV"),
            "Uploaded CV",
            false);

        var action = () => fixture.Service.GenerateHtmlAsync(fixture.UserId, uploaded.Id);

        await action.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Delete_PrimaryResumePromotesNewestRemainingResume()
    {
        var fixture = new ResumeFixture();
        var first = await fixture.Service.CreateAsync(fixture.UserId, fixture.ValidCreate());
        var secondDto = fixture.ValidCreate();
        secondDto.ResumeName = "Second CV";
        var second = await fixture.Service.CreateAsync(fixture.UserId, secondDto);
        fixture.Profile.Resumes.Single(resume => resume.Id == second.Id).CreatedAt = DateTime.UtcNow.AddMinutes(1);

        await fixture.Service.DeleteAsync(fixture.UserId, first.Id);

        fixture.Profile.Resumes.Should().ContainSingle();
        fixture.Profile.Resumes.Single().Id.Should().Be(second.Id);
        fixture.Profile.Resumes.Single().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public async Task Update_SetPrimaryDemotesEveryOtherResume()
    {
        var fixture = new ResumeFixture();
        var first = await fixture.Service.CreateAsync(fixture.UserId, fixture.ValidCreate());
        var secondDto = fixture.ValidCreate();
        secondDto.ResumeName = "Second CV";
        var second = await fixture.Service.CreateAsync(fixture.UserId, secondDto);

        var updated = await fixture.Service.UpdateAsync(
            fixture.UserId,
            second.Id,
            new UpdateResumeDto
            {
                ResumeName = second.ResumeName,
                CareerObjective = second.CareerObjective,
                Languages = second.Languages,
                IsPrimary = true
            });

        updated.IsPrimary.Should().BeTrue();
        fixture.Profile.Resumes.Single(resume => resume.Id == first.Id).IsPrimary.Should().BeFalse();
        fixture.Profile.Resumes.Count(resume => resume.IsPrimary).Should().Be(1);
    }

    [Fact]
    public async Task Get_ForeignUserCannotAccessOwnedResume()
    {
        var fixture = new ResumeFixture();
        var created = await fixture.Service.CreateAsync(fixture.UserId, fixture.ValidCreate());

        var action = () => fixture.Service.GetByIdAsync(Guid.NewGuid(), created.Id);

        await action.Should().ThrowAsync<NotFoundException>();
    }

    private sealed class ResumeFixture
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public JobSeekerProfile Profile { get; }
        public ResumeService Service { get; }
        private FakeResumeRepository ResumeRepository { get; }

        public ResumeFixture(bool completeProfile = false)
        {
            Profile = new JobSeekerProfile
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                Headline = ".NET Developer",
                City = "Colombo",
                Country = "Sri Lanka",
                User = new User
                {
                    Id = UserId,
                    FirstName = "Test",
                    LastName = "Candidate",
                    Email = "candidate@nexhire.local",
                    PhoneNumber = "+94 77 000 0000"
                }
            };

            if (completeProfile)
            {
                Profile.Educations.Add(new Education
                {
                    Id = Guid.NewGuid(),
                    Institution = "NexHire University",
                    Degree = "BSc Software Engineering",
                    StartDate = new DateTime(2020, 1, 1)
                });
                foreach (var name in new[] { "C#", "SQL", "ASP.NET Core" })
                {
                    Profile.CandidateSkills.Add(new CandidateSkill
                    {
                        Id = Guid.NewGuid(),
                        SkillId = Guid.NewGuid(),
                        Skill = new Skill { Id = Guid.NewGuid(), Name = name },
                        ProficiencyLevel = 4
                    });
                }
                Profile.Projects.Add(new Project
                {
                    Id = Guid.NewGuid(),
                    Title = "NexHire",
                    Description = "Recruitment platform"
                });
            }

            var jobSeekerRepository = new FakeJobSeekerRepository(Profile);
            ResumeRepository = new FakeResumeRepository(Profile);
            Service = new ResumeService(
                jobSeekerRepository,
                ResumeRepository,
                new FakeResumeFileStorage());
        }

        public ResumeTemplate AddTemplate(string code, string name)
        {
            var template = new ResumeTemplate
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                TemplateUrl = $"builtin://{code}",
                IsActive = true,
                IsAtsFriendly = true
            };
            ResumeRepository.Templates.Add(template);
            return template;
        }

        public CreateResumeDto ValidCreate(Guid? templateId = null) => new()
        {
            ResumeName = "Developer CV",
            TemplateId = templateId,
            CareerObjective = "I build reliable software systems that solve practical business problems for real users.",
            Languages = new List<string> { "English", "Tamil" },
            LinkedInUrl = "https://www.linkedin.com/in/candidate"
        };
    }

    private sealed class FakeJobSeekerRepository : IJobSeekerRepository
    {
        private readonly JobSeekerProfile _profile;
        public FakeJobSeekerRepository(JobSeekerProfile profile) => _profile = profile;
        public Task<JobSeekerProfile?> GetByIdWithDetailsAsync(Guid profileId) => Task.FromResult<JobSeekerProfile?>(profileId == _profile.Id ? _profile : null);
        public Task<JobSeekerProfile?> GetByUserIdAsync(Guid userId) => Task.FromResult<JobSeekerProfile?>(userId == _profile.UserId ? _profile : null);
        public Task<JobSeekerProfile?> GetByUserIdWithDetailsAsync(Guid userId) => Task.FromResult<JobSeekerProfile?>(userId == _profile.UserId ? _profile : null);
        public Task AddAsync(JobSeekerProfile profile) => Task.CompletedTask;
        public void Update(JobSeekerProfile profile) { }
        public Task<Skill?> GetSkillByNameAsync(string name) => Task.FromResult<Skill?>(null);
        public Task AddSkillAsync(Skill skill) => Task.CompletedTask;
        public Task AddEducationAsync(Education education) => Task.CompletedTask;
        public Task AddExperienceAsync(Experience experience) => Task.CompletedTask;
        public Task AddCandidateSkillAsync(CandidateSkill candidateSkill) => Task.CompletedTask;
        public Task AddProjectAsync(Project project) => Task.CompletedTask;
        public Task AddCertificationAsync(Certification certification) => Task.CompletedTask;
        public void RemoveEducation(Education education) { }
        public void RemoveExperience(Experience experience) { }
        public void RemoveCandidateSkill(CandidateSkill candidateSkill) { }
        public void RemoveProject(Project project) { }
        public void RemoveCertification(Certification certification) { }
        public Task<int> SaveChangesAsync() => Task.FromResult(1);
    }

    private sealed class FakeResumeRepository : IResumeRepository
    {
        private readonly JobSeekerProfile _profile;
        public List<ResumeTemplate> Templates { get; } = new();
        public FakeResumeRepository(JobSeekerProfile profile) => _profile = profile;
        public Task<ResumeTemplate?> GetActiveTemplateByIdAsync(Guid id) => Task.FromResult<ResumeTemplate?>(Templates.FirstOrDefault(template => template.Id == id && template.IsActive));
        public Task<IReadOnlyList<ResumeTemplate>> GetActiveTemplatesAsync() => Task.FromResult<IReadOnlyList<ResumeTemplate>>(Templates.Where(template => template.IsActive).ToList());
        public Task AddAsync(Resume resume)
        {
            _profile.Resumes.Add(resume);
            return Task.CompletedTask;
        }
        public void Remove(Resume resume) => _profile.Resumes.Remove(resume);
        public Task<int> SaveChangesAsync() => Task.FromResult(1);
    }

    private sealed class FakeResumeFileStorage : IResumeFileStorage
    {
        private readonly Dictionary<string, byte[]> _files = new();
        public Task<string> SaveAsync(Guid userId, string extension, byte[] content, CancellationToken cancellationToken = default)
        {
            var key = $"{userId:N}/{Guid.NewGuid():N}{extension}";
            _files[key] = content.ToArray();
            return Task.FromResult(key);
        }
        public Task<byte[]> ReadAsync(string storageKey, CancellationToken cancellationToken = default) =>
            _files.TryGetValue(storageKey, out var content)
                ? Task.FromResult(content.ToArray())
                : Task.FromException<byte[]>(new FileNotFoundException());
        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            _files.Remove(storageKey);
            return Task.CompletedTask;
        }
    }
}

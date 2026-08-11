using AutoMapper;
using NexHire.Application.Common.Exceptions;
using NexHire.Application.DTOs.JobSeeker;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Validators.JobSeeker;
using NexHire.Domain.Entities;

namespace NexHire.Application.Services;

public class JobSeekerService : IJobSeekerService
{
    private readonly IJobSeekerRepository _repository;
    private readonly IMapper _mapper;

    public JobSeekerService(IJobSeekerRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<JobSeekerProfileResponseDto> GetMyProfileAsync(Guid userId)
    {
        var profile = await GetOwnedProfileAsync(userId);
        return _mapper.Map<JobSeekerProfileResponseDto>(profile);
    }

    public async Task<PublicJobSeekerProfileResponseDto> GetPublicProfileAsync(Guid profileId)
    {
        var profile = await _repository.GetByIdWithDetailsAsync(profileId)
            ?? throw new NotFoundException("Job seeker profile not found.");

        if (!profile.IsProfilePublic)
            throw new NotFoundException("Job seeker profile not found.");

        return _mapper.Map<PublicJobSeekerProfileResponseDto>(profile);
    }

    public async Task<JobSeekerProfileResponseDto> CreateProfileAsync(Guid userId, CreateJobSeekerProfileDto dto)
    {
        ValidateDto(new CreateJobSeekerProfileValidator(), dto);

        if (await _repository.GetByUserIdAsync(userId) is not null)
            throw new BusinessRuleException("A job seeker profile already exists for this user.");

        var profile = _mapper.Map<JobSeekerProfile>(dto);
        profile.Id = Guid.NewGuid();
        profile.UserId = userId;
        profile.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(profile);
        await _repository.SaveChangesAsync();

        return _mapper.Map<JobSeekerProfileResponseDto>(profile);
    }

    public async Task<JobSeekerProfileResponseDto> UpdateProfileAsync(Guid userId, UpdateJobSeekerProfileDto dto)
    {
        ValidateDto(new UpdateJobSeekerProfileValidator(), dto);
        var profile = await GetOwnedProfileAsync(userId);

        var years = dto.YearsOfExperience ?? profile.YearsOfExperience;
        var dob = dto.ClearDateOfBirth
            ? null
            : dto.DateOfBirth ?? profile.DateOfBirth;
        var minSalary = dto.ClearExpectedSalaryMin
            ? null
            : dto.ExpectedSalaryMin ?? profile.ExpectedSalaryMin;
        var maxSalary = dto.ClearExpectedSalaryMax
            ? null
            : dto.ExpectedSalaryMax ?? profile.ExpectedSalaryMax;
        ValidateProfile(years, dob, minSalary, maxSalary);

        _mapper.Map(dto, profile);
        if (dto.ClearDateOfBirth)
            profile.DateOfBirth = null;
        if (dto.ClearExpectedSalaryMin)
            profile.ExpectedSalaryMin = null;
        if (dto.ClearExpectedSalaryMax)
            profile.ExpectedSalaryMax = null;
        profile.UpdatedAt = DateTime.UtcNow;
        _repository.Update(profile);
        await _repository.SaveChangesAsync();

        return _mapper.Map<JobSeekerProfileResponseDto>(profile);
    }

    public async Task<EducationResponseDto> AddEducationAsync(Guid userId, AddEducationDto dto)
    {
        ValidateDto(new AddEducationValidator(), dto);
        var profile = await GetOwnedProfileAsync(userId);
        var education = _mapper.Map<Education>(dto);
        education.Id = Guid.NewGuid();
        education.JobSeekerProfileId = profile.Id;
        await _repository.AddEducationAsync(education);
        await _repository.SaveChangesAsync();
        return _mapper.Map<EducationResponseDto>(education);
    }

    public async Task<EducationResponseDto> UpdateEducationAsync(Guid userId, Guid educationId, AddEducationDto dto)
    {
        ValidateDto(new AddEducationValidator(), dto);
        var profile = await GetOwnedProfileAsync(userId);
        var education = profile.Educations.FirstOrDefault(x => x.Id == educationId)
            ?? throw new NotFoundException("Education record not found.");
        _mapper.Map(dto, education);
        await _repository.SaveChangesAsync();
        return _mapper.Map<EducationResponseDto>(education);
    }

    public async Task DeleteEducationAsync(Guid userId, Guid educationId)
    {
        var profile = await GetOwnedProfileAsync(userId);
        var education = profile.Educations.FirstOrDefault(x => x.Id == educationId)
            ?? throw new NotFoundException("Education record not found.");
        _repository.RemoveEducation(education);
        await _repository.SaveChangesAsync();
    }

    public async Task<ExperienceResponseDto> AddExperienceAsync(Guid userId, AddExperienceDto dto)
    {
        ValidateDto(new AddExperienceValidator(), dto);
        var profile = await GetOwnedProfileAsync(userId);
        var experience = _mapper.Map<Experience>(dto);
        experience.Id = Guid.NewGuid();
        experience.JobSeekerProfileId = profile.Id;
        await _repository.AddExperienceAsync(experience);
        await _repository.SaveChangesAsync();
        return _mapper.Map<ExperienceResponseDto>(experience);
    }

    public async Task<ExperienceResponseDto> UpdateExperienceAsync(Guid userId, Guid experienceId, AddExperienceDto dto)
    {
        ValidateDto(new AddExperienceValidator(), dto);
        var profile = await GetOwnedProfileAsync(userId);
        var experience = profile.Experiences.FirstOrDefault(x => x.Id == experienceId)
            ?? throw new NotFoundException("Experience record not found.");
        _mapper.Map(dto, experience);
        await _repository.SaveChangesAsync();
        return _mapper.Map<ExperienceResponseDto>(experience);
    }

    public async Task DeleteExperienceAsync(Guid userId, Guid experienceId)
    {
        var profile = await GetOwnedProfileAsync(userId);
        var experience = profile.Experiences.FirstOrDefault(x => x.Id == experienceId)
            ?? throw new NotFoundException("Experience record not found.");
        _repository.RemoveExperience(experience);
        await _repository.SaveChangesAsync();
    }

    public async Task<CandidateSkillResponseDto> AddSkillAsync(Guid userId, AddSkillDto dto)
    {
        ValidateDto(new AddSkillValidator(), dto);
        var profile = await GetOwnedProfileAsync(userId);
        var skill = await GetOrCreateSkillAsync(dto.SkillName);

        if (profile.CandidateSkills.Any(x => x.SkillId == skill.Id || x.Skill.Name.Equals(skill.Name, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessRuleException("This skill is already on the job seeker profile.");

        var candidateSkill = new CandidateSkill
        {
            Id = Guid.NewGuid(),
            JobSeekerProfileId = profile.Id,
            SkillId = skill.Id,
            Skill = skill,
            ProficiencyLevel = dto.ProficiencyLevel,
            YearsOfExperience = dto.YearsOfExperience
        };

        await _repository.AddCandidateSkillAsync(candidateSkill);
        await _repository.SaveChangesAsync();
        return _mapper.Map<CandidateSkillResponseDto>(candidateSkill);
    }

    public async Task<CandidateSkillResponseDto> UpdateSkillAsync(Guid userId, Guid candidateSkillId, AddSkillDto dto)
    {
        ValidateDto(new AddSkillValidator(), dto);
        var profile = await GetOwnedProfileAsync(userId);
        var candidateSkill = profile.CandidateSkills.FirstOrDefault(x => x.Id == candidateSkillId)
            ?? throw new NotFoundException("Candidate skill not found.");

        var skill = await GetOrCreateSkillAsync(dto.SkillName);
        if (profile.CandidateSkills.Any(x => x.Id != candidateSkillId && x.SkillId == skill.Id))
            throw new BusinessRuleException("This skill is already on the job seeker profile.");

        candidateSkill.SkillId = skill.Id;
        candidateSkill.Skill = skill;
        candidateSkill.ProficiencyLevel = dto.ProficiencyLevel;
        candidateSkill.YearsOfExperience = dto.YearsOfExperience;
        await _repository.SaveChangesAsync();
        return _mapper.Map<CandidateSkillResponseDto>(candidateSkill);
    }

    public async Task DeleteSkillAsync(Guid userId, Guid candidateSkillId)
    {
        var profile = await GetOwnedProfileAsync(userId);
        var candidateSkill = profile.CandidateSkills.FirstOrDefault(x => x.Id == candidateSkillId)
            ?? throw new NotFoundException("Candidate skill not found.");
        _repository.RemoveCandidateSkill(candidateSkill);
        await _repository.SaveChangesAsync();
    }

    public async Task<ProjectResponseDto> AddProjectAsync(Guid userId, AddProjectDto dto)
    {
        ValidateDto(new AddProjectValidator(), dto);
        var profile = await GetOwnedProfileAsync(userId);
        var project = _mapper.Map<Project>(dto);
        project.Id = Guid.NewGuid();
        project.JobSeekerProfileId = profile.Id;
        await _repository.AddProjectAsync(project);
        await _repository.SaveChangesAsync();
        return _mapper.Map<ProjectResponseDto>(project);
    }

    public async Task<ProjectResponseDto> UpdateProjectAsync(Guid userId, Guid projectId, AddProjectDto dto)
    {
        ValidateDto(new AddProjectValidator(), dto);
        var profile = await GetOwnedProfileAsync(userId);
        var project = profile.Projects.FirstOrDefault(x => x.Id == projectId)
            ?? throw new NotFoundException("Project record not found.");
        _mapper.Map(dto, project);
        await _repository.SaveChangesAsync();
        return _mapper.Map<ProjectResponseDto>(project);
    }

    public async Task DeleteProjectAsync(Guid userId, Guid projectId)
    {
        var profile = await GetOwnedProfileAsync(userId);
        var project = profile.Projects.FirstOrDefault(x => x.Id == projectId)
            ?? throw new NotFoundException("Project record not found.");
        _repository.RemoveProject(project);
        await _repository.SaveChangesAsync();
    }

    public async Task<CertificationResponseDto> AddCertificationAsync(Guid userId, AddCertificationDto dto)
    {
        ValidateDto(new AddCertificationValidator(), dto);
        var profile = await GetOwnedProfileAsync(userId);
        var certification = _mapper.Map<Certification>(dto);
        certification.Id = Guid.NewGuid();
        certification.JobSeekerProfileId = profile.Id;
        await _repository.AddCertificationAsync(certification);
        await _repository.SaveChangesAsync();
        return _mapper.Map<CertificationResponseDto>(certification);
    }

    public async Task<CertificationResponseDto> UpdateCertificationAsync(Guid userId, Guid certificationId, AddCertificationDto dto)
    {
        ValidateDto(new AddCertificationValidator(), dto);
        var profile = await GetOwnedProfileAsync(userId);
        var certification = profile.Certifications.FirstOrDefault(x => x.Id == certificationId)
            ?? throw new NotFoundException("Certification record not found.");
        _mapper.Map(dto, certification);
        await _repository.SaveChangesAsync();
        return _mapper.Map<CertificationResponseDto>(certification);
    }

    public async Task DeleteCertificationAsync(Guid userId, Guid certificationId)
    {
        var profile = await GetOwnedProfileAsync(userId);
        var certification = profile.Certifications.FirstOrDefault(x => x.Id == certificationId)
            ?? throw new NotFoundException("Certification record not found.");
        _repository.RemoveCertification(certification);
        await _repository.SaveChangesAsync();
    }

    private async Task<JobSeekerProfile> GetOwnedProfileAsync(Guid userId) =>
        await _repository.GetByUserIdWithDetailsAsync(userId)
        ?? throw new NotFoundException("Job seeker profile not found. Create the profile first.");

    private async Task<Skill> GetOrCreateSkillAsync(string skillName)
    {
        var cleaned = skillName.Trim();
        var skill = await _repository.GetSkillByNameAsync(cleaned);
        if (skill is not null)
            return skill;

        skill = new Skill { Id = Guid.NewGuid(), Name = cleaned };
        await _repository.AddSkillAsync(skill);
        return skill;
    }

    private static void ValidateProfile(int years, DateTime? dateOfBirth, decimal? minSalary, decimal? maxSalary)
    {
        if (years is < 0 or > 60)
            throw new ValidationException("Years of experience must be between 0 and 60.");
        if (dateOfBirth.HasValue && dateOfBirth.Value.Date > DateTime.UtcNow.Date)
            throw new ValidationException("Date of birth cannot be in the future.");
        if (minSalary.HasValue && minSalary < 0 || maxSalary.HasValue && maxSalary < 0)
            throw new ValidationException("Expected salary cannot be negative.");
        if (minSalary.HasValue && maxSalary.HasValue && minSalary > maxSalary)
            throw new ValidationException("Minimum expected salary cannot be greater than maximum expected salary.");
    }

    private static void ValidateDto<T>(FluentValidation.IValidator<T> validator, T dto)
    {
        var result = validator.Validate(dto);
        if (result.IsValid)
            return;

        var message = string.Join(
            " ",
            result.Errors
                .Select(error => error.ErrorMessage)
                .Distinct(StringComparer.Ordinal));
        throw new ValidationException(message);
    }
}

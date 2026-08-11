using NexHire.Application.DTOs.JobSeeker;

namespace NexHire.Application.Interfaces.Services;

public interface IJobSeekerService
{
    Task<JobSeekerProfileResponseDto> GetMyProfileAsync(Guid userId);
    Task<PublicJobSeekerProfileResponseDto> GetPublicProfileAsync(Guid profileId);
    Task<JobSeekerProfileResponseDto> CreateProfileAsync(Guid userId, CreateJobSeekerProfileDto dto);
    Task<JobSeekerProfileResponseDto> UpdateProfileAsync(Guid userId, UpdateJobSeekerProfileDto dto);

    Task<EducationResponseDto> AddEducationAsync(Guid userId, AddEducationDto dto);
    Task<EducationResponseDto> UpdateEducationAsync(Guid userId, Guid educationId, AddEducationDto dto);
    Task DeleteEducationAsync(Guid userId, Guid educationId);

    Task<ExperienceResponseDto> AddExperienceAsync(Guid userId, AddExperienceDto dto);
    Task<ExperienceResponseDto> UpdateExperienceAsync(Guid userId, Guid experienceId, AddExperienceDto dto);
    Task DeleteExperienceAsync(Guid userId, Guid experienceId);

    Task<CandidateSkillResponseDto> AddSkillAsync(Guid userId, AddSkillDto dto);
    Task<CandidateSkillResponseDto> UpdateSkillAsync(Guid userId, Guid candidateSkillId, AddSkillDto dto);
    Task DeleteSkillAsync(Guid userId, Guid candidateSkillId);

    Task<ProjectResponseDto> AddProjectAsync(Guid userId, AddProjectDto dto);
    Task<ProjectResponseDto> UpdateProjectAsync(Guid userId, Guid projectId, AddProjectDto dto);
    Task DeleteProjectAsync(Guid userId, Guid projectId);

    Task<CertificationResponseDto> AddCertificationAsync(Guid userId, AddCertificationDto dto);
    Task<CertificationResponseDto> UpdateCertificationAsync(Guid userId, Guid certificationId, AddCertificationDto dto);
    Task DeleteCertificationAsync(Guid userId, Guid certificationId);
}

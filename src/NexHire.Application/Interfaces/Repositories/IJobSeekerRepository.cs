using NexHire.Domain.Entities;

namespace NexHire.Application.Interfaces.Repositories;

public interface IJobSeekerRepository
{
    Task<JobSeekerProfile?> GetByIdWithDetailsAsync(Guid profileId);
    Task<JobSeekerProfile?> GetByUserIdAsync(Guid userId);
    Task<JobSeekerProfile?> GetByUserIdWithDetailsAsync(Guid userId);
    Task AddAsync(JobSeekerProfile profile);
    void Update(JobSeekerProfile profile);

    Task<Skill?> GetSkillByNameAsync(string name);
    Task AddSkillAsync(Skill skill);

    Task AddEducationAsync(Education education);
    Task AddExperienceAsync(Experience experience);
    Task AddCandidateSkillAsync(CandidateSkill candidateSkill);
    Task AddProjectAsync(Project project);
    Task AddCertificationAsync(Certification certification);

    void RemoveEducation(Education education);
    void RemoveExperience(Experience experience);
    void RemoveCandidateSkill(CandidateSkill candidateSkill);
    void RemoveProject(Project project);
    void RemoveCertification(Certification certification);

    Task<int> SaveChangesAsync();
}

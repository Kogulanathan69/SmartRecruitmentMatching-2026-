using Microsoft.EntityFrameworkCore;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Domain.Entities;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Repositories;

public class JobSeekerRepository : IJobSeekerRepository
{
    private readonly AppDbContext _context;

    public JobSeekerRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<JobSeekerProfile?> GetByIdWithDetailsAsync(Guid profileId) =>
        DetailsQuery().FirstOrDefaultAsync(p => p.Id == profileId);

    public Task<JobSeekerProfile?> GetByUserIdAsync(Guid userId) =>
        _context.Set<JobSeekerProfile>()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

    public Task<JobSeekerProfile?> GetByUserIdWithDetailsAsync(Guid userId) =>
        DetailsQuery().FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task AddAsync(JobSeekerProfile profile) =>
        await _context.Set<JobSeekerProfile>().AddAsync(profile);

    public void Update(JobSeekerProfile profile) =>
        _context.Set<JobSeekerProfile>().Update(profile);

    public Task<Skill?> GetSkillByNameAsync(string name)
    {
        var normalized = name.Trim().ToLower();
        return _context.Set<Skill>()
            .FirstOrDefaultAsync(s => s.Name.ToLower() == normalized);
    }

    public async Task AddSkillAsync(Skill skill) =>
        await _context.Set<Skill>().AddAsync(skill);

    public async Task AddEducationAsync(Education education) =>
        await _context.Set<Education>().AddAsync(education);

    public async Task AddExperienceAsync(Experience experience) =>
        await _context.Set<Experience>().AddAsync(experience);

    public async Task AddCandidateSkillAsync(CandidateSkill candidateSkill) =>
        await _context.Set<CandidateSkill>().AddAsync(candidateSkill);

    public async Task AddProjectAsync(Project project) =>
        await _context.Set<Project>().AddAsync(project);

    public async Task AddCertificationAsync(Certification certification) =>
        await _context.Set<Certification>().AddAsync(certification);

    public void RemoveEducation(Education education) =>
        _context.Set<Education>().Remove(education);

    public void RemoveExperience(Experience experience) =>
        _context.Set<Experience>().Remove(experience);

    public void RemoveCandidateSkill(CandidateSkill candidateSkill) =>
        _context.Set<CandidateSkill>().Remove(candidateSkill);

    public void RemoveProject(Project project) =>
        _context.Set<Project>().Remove(project);

    public void RemoveCertification(Certification certification) =>
        _context.Set<Certification>().Remove(certification);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

    private IQueryable<JobSeekerProfile> DetailsQuery() =>
        _context.Set<JobSeekerProfile>()
            .AsSplitQuery()
            .Include(p => p.User)
            .Include(p => p.Educations)
            .Include(p => p.Experiences)
            .Include(p => p.CandidateSkills).ThenInclude(cs => cs.Skill)
            .Include(p => p.Projects)
            .Include(p => p.Certifications)
            .Include(p => p.Resumes).ThenInclude(r => r.ResumeTemplate);
}

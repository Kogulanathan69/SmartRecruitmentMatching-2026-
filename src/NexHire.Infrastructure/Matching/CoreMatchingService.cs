using Microsoft.EntityFrameworkCore;
using NexHire.Application.Common.Exceptions;
using NexHire.Application.DTOs.Matching;
using NexHire.Application.Interfaces.Services;
using NexHire.Domain.Entities;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Matching;

public class CoreMatchingService : ICoreMatchingService
{
    private readonly AppDbContext _db;

    public CoreMatchingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CoreMatchDto> CalculateAsync(
        Guid candidateProfileId,
        Guid jobId)
    {
        var job = await _db.Jobs
            .AsNoTracking()
            .Include(x => x.RequiredSkills)
            .ThenInclude(x => x.Skill)
            .FirstOrDefaultAsync(x => x.Id == jobId)
            ?? throw new NotFoundException("Job not found.");

        var candidate = await _db.JobSeekerProfiles
            .AsNoTracking()
            .Include(x => x.CandidateSkills)
            .ThenInclude(x => x.Skill)
            .Include(x => x.Educations)
            .FirstOrDefaultAsync(x => x.Id == candidateProfileId)
            ?? throw new NotFoundException(
                "Candidate profile not found.");

        var requiredSkills =
            job.RequiredSkills.ToList();

        var skillGaps = new List<string>();

        var matchedSkills = 0;

        foreach (var required in requiredSkills)
        {
            var candidateSkill =
                candidate.CandidateSkills
                    .FirstOrDefault(x =>
                        x.SkillId == required.SkillId);

            if (candidateSkill != null &&
                candidateSkill.ProficiencyLevel >=
                required.MinimumProficiencyLevel)
            {
                matchedSkills++;
            }
            else
            {
                skillGaps.Add(
                    required.Skill?.Name ??
                    required.SkillId.ToString());
            }
        }

        var skillScore =
            requiredSkills.Count == 0
                ? 100m
                : Math.Round(
                    100m *
                    matchedSkills /
                    requiredSkills.Count,
                    2);

        var minimumExperience =
            job.ExperienceMinYears;

        var experienceScore =
            minimumExperience <= 0
                ? 100m
                : Math.Min(
                    100m,
                    Math.Round(
                        100m *
                        candidate.YearsOfExperience /
                        minimumExperience,
                        2));

        decimal educationScore;

        if (job.MinimumEducationLevel <= 0)
        {
            educationScore = 100m;
        }
        else
        {
            educationScore =
                candidate.Educations.Count > 0
                    ? 100m
                    : 0m;
        }

        decimal locationScore;

        if (job.IsRemote)
        {
            locationScore = 100m;
        }
        else
        {
            var cityMatched =
                !string.IsNullOrWhiteSpace(job.LocationCity) &&
                string.Equals(
                    candidate.City,
                    job.LocationCity,
                    StringComparison.OrdinalIgnoreCase);

            var countryMatched =
                !string.IsNullOrWhiteSpace(job.LocationCountry) &&
                string.Equals(
                    candidate.Country,
                    job.LocationCountry,
                    StringComparison.OrdinalIgnoreCase);

            locationScore =
                cityMatched || countryMatched
                    ? 100m
                    : 0m;
        }

        var totalScore = Math.Round(
            skillScore * 0.55m +
            experienceScore * 0.20m +
            educationScore * 0.15m +
            locationScore * 0.10m,
            2);

        return new CoreMatchDto
        {
            JobId = jobId,
            CandidateProfileId =
                candidateProfileId,

            TotalScore =
                totalScore,

            RequiredSkillsScore =
                skillScore,

            ExperienceScore =
                experienceScore,

            EducationScore =
                educationScore,

            LocationScore =
                locationScore,

            IsEligible =
                skillGaps.Count == 0,

            SkillGaps =
                skillGaps,

            Explanation =
                $"Required skills 55%, experience 20%, education 15%, location 10%. Total {totalScore:0.##}%."
        };
    }
}
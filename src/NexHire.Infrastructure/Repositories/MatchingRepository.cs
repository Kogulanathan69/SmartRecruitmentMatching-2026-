using Microsoft.EntityFrameworkCore;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Matching;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Repositories;

public class MatchingRepository : IMatchingRepository
{
    private readonly AppDbContext _context;

    public MatchingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MatchingCalculationInput?> GetMatchingCalculationInputAsync(
        Guid jobId,
        Guid jobSeekerProfileId,
        CancellationToken cancellationToken = default)
    {
        if (jobId == Guid.Empty || jobSeekerProfileId == Guid.Empty)
        {
            return null;
        }

        var job = await _context.Jobs
            .AsNoTracking()
            .Include(j => j.RequiredSkills)
                .ThenInclude(rs => rs.Skill)
            .Include(j => j.PreferredSkills)
                .ThenInclude(ps => ps.Skill)
            .SingleOrDefaultAsync(
                j => j.Id == jobId,
                cancellationToken);

        if (job is null)
        {
            return null;
        }

        var profile = await _context.JobSeekerProfiles
            .AsNoTracking()
            .Include(p => p.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
            .Include(p => p.Educations)
            .Include(p => p.Experiences)
            .Include(p => p.Certifications)
            .Include(p => p.Projects)
            .Include(p => p.Resumes)
            .SingleOrDefaultAsync(
                p => p.Id == jobSeekerProfileId,
                cancellationToken);

        if (profile is null)
        {
            return null;
        }

        var matchingRule = await _context.MatchingRules
            .AsNoTracking()
            .SingleOrDefaultAsync(
                r => r.IsActive,
                cancellationToken);

        if (matchingRule is null)
        {
            throw new InvalidOperationException(
                "No active matching rule is configured.");
        }

        if (!matchingRule.HasValidTotalWeight())
        {
            throw new InvalidOperationException(
                "The active matching rule weights must total 100.");
        }

        var mandatorySkills = job.RequiredSkills
            .Select(skill => new RequiredSkillInput
            {
                SkillName = skill.Skill.Name,
                MinimumProficiencyLevel =
                    skill.MinimumProficiencyLevel
            })
            .ToList();

        var preferredSkills = job.PreferredSkills
            .Select(skill => new RequiredSkillInput
            {
                SkillName = skill.Skill.Name,
                MinimumProficiencyLevel =
                    skill.MinimumProficiencyLevel
            })
            .ToList();

        var candidateSkills = profile.CandidateSkills
            .Select(skill => new CandidateSkillInput
            {
                SkillName = skill.Skill.Name,
                ProficiencyLevel = skill.ProficiencyLevel
            })
            .ToList();

        var candidateExperienceYears =
            Math.Max(0, profile.YearsOfExperience);

        var candidateEducationLevel =
            profile.Educations.Count == 0
                ? 0
                : profile.Educations.Max(
                    education => education.EducationLevel);

        var requiredCertifications =
            ParseDelimitedValues(job.RequiredCertifications);

        var candidateCertifications =
            profile.Certifications
                .Where(certification =>
                    !string.IsNullOrWhiteSpace(
                        certification.Name))
                .Select(certification =>
                    certification.Name.Trim())
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var jobLocation = job.IsRemote
            ? string.Empty
            : GetLocation(
                job.LocationCity,
                job.LocationCountry);

        var candidateLocation =
            GetLocation(
                profile.City,
                profile.Country);

        var isJobAvailable =
            job.Status == JobStatus.Published &&
            (!job.ClosingDate.HasValue ||
             job.ClosingDate.Value >= DateTime.UtcNow);

        var unavailableReason =
            GetJobUnavailableReason(job);

        var profileCompletionPercentage =
            CalculateProfileCompletion(profile);

        return new MatchingCalculationInput
        {
            JobId = job.Id,
            JobSeekerProfileId = profile.Id,

            Eligibility = new EligibilityInput
            {
                IsJobAvailable = isJobAvailable,
                JobUnavailableReason = unavailableReason,

                CandidateExperienceYears =
                    candidateExperienceYears,

                MinimumExperienceYears =
                    job.ExperienceMinYears,

                CandidateEducationLevel =
                    candidateEducationLevel,

                MinimumEducationLevel =
                    job.MinimumEducationLevel,

                MandatorySkills = mandatorySkills,
                CandidateSkills = candidateSkills
            },

            Skills = new SkillMatchInput
            {
                MandatorySkills = mandatorySkills,
                PreferredSkills = preferredSkills,
                CandidateSkills = candidateSkills
            },

            Experience = new ExperienceMatchInput
            {
                CandidateExperienceYears =
                    candidateExperienceYears,

                RequiredExperienceYears =
                    job.ExperienceMinYears
            },

            Education = new EducationMatchInput
            {
                CandidateEducationLevel =
                    candidateEducationLevel,

                RequiredEducationLevel =
                    job.MinimumEducationLevel
            },

            Certification = new CertificationMatchInput
            {
                RequiredCertifications =
                    requiredCertifications,

                CandidateCertifications =
                    candidateCertifications
            },

            Location = new LocationMatchInput
            {
                CandidateLocation =
                    candidateLocation,

                JobLocation =
                    jobLocation
            },

            Projects = new ProjectMatchInput
            {
                // Current project model does not yet contain
                // explicit job-skill relevance tags.
                // Therefore all projects saved in the candidate
                // professional profile are counted here.
                RelevantProjectCount =
                    profile.Projects.Count,

                TargetProjectCount =
                    job.TargetProjectCount
            },

            ProfileCompletion =
                new ProfileCompletionMatchInput
                {
                    CompletionPercentage =
                        profileCompletionPercentage
                },

            SkillsWeight =
                matchingRule.SkillsWeight,

            ExperienceWeight =
                matchingRule.ExperienceWeight,

            EducationWeight =
                matchingRule.EducationWeight,

            CertificationWeight =
                matchingRule.CertificationWeight,

            LocationWeight =
                matchingRule.LocationWeight,

            ProjectsWeight =
                matchingRule.ProjectsWeight,

            ProfileCompletionWeight =
                matchingRule.ProfileCompletionWeight
        };
    }

    public async Task SaveMatchResultAsync(
        MatchingCalculationResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        var matchResult = new MatchResult
        {
            Id = Guid.NewGuid(),
            JobSeekerProfileId = result.JobSeekerProfileId,
            JobId = result.JobId,
            TotalScore = result.TotalScore,
            IsEligible = result.IsEligible,
            Recommendation = result.Recommendation,
            Summary = result.Summary,
            CalculatedAtUtc = DateTime.UtcNow
        };

        foreach (var detail in result.ScoreDetails)
        {
            matchResult.ScoreDetails.Add(
                new MatchScoreDetail
                {
                    Id = Guid.NewGuid(),
                    MatchResultId = matchResult.Id,
                    Category = detail.Category,
                    RawScore = detail.RawScore,
                    Weight = detail.Weight,
                    WeightedPoints = detail.WeightedPoints,
                    MaximumWeightedPoints =
                        detail.MaximumWeightedPoints,
                    Status = detail.Status,
                    Explanation = detail.Explanation
                });
        }

        await _context.MatchResults.AddAsync(
            matchResult,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<CandidateRankingInput>>
        GetRankingInputsForJobAsync(
            Guid jobId,
            CancellationToken cancellationToken = default)
    {
        if (jobId == Guid.Empty)
        {
            return Array.Empty<CandidateRankingInput>();
        }

        // Only active applications are ranked.
        // Rejected and withdrawn applications are excluded.
        var applications =
            await (
                from application in
                    _context.JobApplications.AsNoTracking()

                join profile in
                    _context.JobSeekerProfiles.AsNoTracking()

                    on application.CandidateId
                    equals profile.UserId

                where application.VacancyId == jobId
                      && application.Status !=
                         ApplicationStatus.Rejected.ToString()
                      && application.Status !=
                         ApplicationStatus.Withdrawn.ToString()

                select new
                {
                    ApplicationId =
                        application.JobApplicationId,

                    JobSeekerProfileId =
                        profile.Id
                })
                .ToListAsync(cancellationToken);

        if (applications.Count == 0)
        {
            return Array.Empty<CandidateRankingInput>();
        }

        var profileIds =
            applications
                .Select(application =>
                    application.JobSeekerProfileId)
                .Distinct()
                .ToList();

        // Load historical calculations once and select the
        // latest result for every candidate profile.
        var matchResults =
            await _context.MatchResults
                .AsNoTracking()
                .Where(result =>
                    result.JobId == jobId &&
                    profileIds.Contains(
                        result.JobSeekerProfileId))
                .ToListAsync(cancellationToken);

        var latestByProfile =
            matchResults
                .GroupBy(result =>
                    result.JobSeekerProfileId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderByDescending(result =>
                            result.CalculatedAtUtc)
                        .ThenByDescending(result =>
                            result.Id)
                        .First());

        var rankingInputs =
            new List<CandidateRankingInput>();

        foreach (var application in applications)
        {
            if (!latestByProfile.TryGetValue(
                    application.JobSeekerProfileId,
                    out var latest))
            {
                // Candidate has not been matched yet.
                continue;
            }

            // Mandatory eligibility failure means the
            // candidate is not included in top-candidate ranking.
            if (!latest.IsEligible)
            {
                continue;
            }

            rankingInputs.Add(
                new CandidateRankingInput
                {
                    ApplicationId =
                        application.ApplicationId,

                    JobSeekerProfileId =
                        application.JobSeekerProfileId,

                    TotalScore =
                        latest.TotalScore,

                    IsEligible =
                        latest.IsEligible,

                    Recommendation =
                        latest.Recommendation
                });
        }

        return rankingInputs;
    }

    public async Task<IReadOnlyList<CandidateComparisonInput>>
        GetComparisonInputsForJobAsync(
            Guid jobId,
            IReadOnlyCollection<Guid> applicationIds,
            CancellationToken cancellationToken = default)
    {
        if (jobId == Guid.Empty ||
            applicationIds.Count == 0)
        {
            return Array.Empty<CandidateComparisonInput>();
        }

        var requestedIds =
            applicationIds
                .Distinct()
                .ToList();

        // Only applications belonging to this job and still
        // active in the recruitment process can be compared.
        var applications =
            await (
                from application in
                    _context.JobApplications.AsNoTracking()

                join profile in
                    _context.JobSeekerProfiles.AsNoTracking()

                    on application.CandidateId
                    equals profile.UserId

                where application.VacancyId == jobId
                      && requestedIds.Contains(
                          application.JobApplicationId)
                      && application.Status !=
                         ApplicationStatus.Rejected.ToString()
                      && application.Status !=
                         ApplicationStatus.Withdrawn.ToString()

                select new
                {
                    ApplicationId =
                        application.JobApplicationId,

                    JobSeekerProfileId =
                        profile.Id
                })
                .ToListAsync(cancellationToken);

        if (applications.Count == 0)
        {
            return Array.Empty<CandidateComparisonInput>();
        }

        var profileIds =
            applications
                .Select(application =>
                    application.JobSeekerProfileId)
                .Distinct()
                .ToList();

        // Comparison uses the latest saved calculation
        // for every selected candidate.
        var savedResults =
            await _context.MatchResults
                .AsNoTracking()
                .Include(result =>
                    result.ScoreDetails)
                .Where(result =>
                    result.JobId == jobId &&
                    profileIds.Contains(
                        result.JobSeekerProfileId))
                .ToListAsync(cancellationToken);

        var latestByProfile =
            savedResults
                .GroupBy(result =>
                    result.JobSeekerProfileId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderByDescending(result =>
                            result.CalculatedAtUtc)
                        .ThenByDescending(result =>
                            result.Id)
                        .First());

        var comparisonInputs =
            new List<CandidateComparisonInput>();

        foreach (var application in applications)
        {
            if (!latestByProfile.TryGetValue(
                    application.JobSeekerProfileId,
                    out var latest))
            {
                continue;
            }

            comparisonInputs.Add(
                new CandidateComparisonInput
                {
                    ApplicationId =
                        application.ApplicationId,

                    JobSeekerProfileId =
                        application.JobSeekerProfileId,

                    TotalScore =
                        latest.TotalScore,

                    IsEligible =
                        latest.IsEligible,

                    Recommendation =
                        latest.Recommendation,

                    ScoreDetails =
                        latest.ScoreDetails
                            .Select(detail =>
                                new WeightedCategoryScoreResult
                                {
                                    Category =
                                        detail.Category,

                                    RawScore =
                                        detail.RawScore,

                                    Weight =
                                        detail.Weight,

                                    WeightedPoints =
                                        detail.WeightedPoints,

                                    MaximumWeightedPoints =
                                        detail.MaximumWeightedPoints,

                                    Status =
                                        detail.Status,

                                    Explanation =
                                        detail.Explanation
                                })
                            .ToList()
                });
        }

        return comparisonInputs;
    }

    private static List<string> ParseDelimitedValues(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new List<string>();
        }

        return value
            .Split(
                new[] { ',', ';', '|' },
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string GetLocation(
        string? city,
        string? country)
    {
        if (!string.IsNullOrWhiteSpace(city))
        {
            return city.Trim();
        }

        return country?.Trim() ?? string.Empty;
    }

    private static string GetJobUnavailableReason(
        Job job)
    {
        if (job.Status != JobStatus.Published)
        {
            return $"Job is currently {job.Status}.";
        }

        if (job.ClosingDate.HasValue &&
            job.ClosingDate.Value < DateTime.UtcNow)
        {
            return "The job closing date has passed.";
        }

        return string.Empty;
    }

    private static decimal CalculateProfileCompletion(
        JobSeekerProfile profile)
    {
        // Only professional profile information is used here.
        // Sensitive fields such as gender and date of birth
        // are intentionally excluded from matching.

        var completionChecks = new[]
        {
            !string.IsNullOrWhiteSpace(profile.Headline),
            !string.IsNullOrWhiteSpace(profile.Summary),

            !string.IsNullOrWhiteSpace(profile.City) ||
            !string.IsNullOrWhiteSpace(profile.Country),

            profile.CandidateSkills.Count > 0,
            profile.Educations.Count > 0,

            profile.YearsOfExperience > 0 ||
            profile.Experiences.Count > 0,

            profile.Resumes.Count > 0
        };

        var completedItems =
            completionChecks.Count(isCompleted => isCompleted);

        return Math.Round(
            (decimal)completedItems /
            completionChecks.Length * 100m,
            2);
    }
}

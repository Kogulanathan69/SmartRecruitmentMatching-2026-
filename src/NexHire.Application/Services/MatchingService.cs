using NexHire.Application.DTOs.Matching;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Matching;
using NexHire.Application.Mappings;

namespace NexHire.Application.Services;

/// <summary>
/// Coordinates the complete candidate-to-job matching process.
/// </summary>
public class MatchingService : IMatchingService
{
    private readonly IMatchingEngine _matchingEngine;
    private readonly IMatchingRepository? _matchingRepository;
    private readonly ICurrentUserService? _currentUserService;

    /// <summary>
    /// Constructor kept for existing unit tests and pure
    /// in-memory matching calculations.
    /// </summary>
    public MatchingService(
        IMatchingEngine matchingEngine)
    {
        _matchingEngine =
            matchingEngine
            ?? throw new ArgumentNullException(
                nameof(matchingEngine));
    }

    /// <summary>
    /// Production constructor used by dependency injection.
    /// Provides both the matching engine and database repository.
    /// </summary>
    public MatchingService(
        IMatchingEngine matchingEngine,
        IMatchingRepository matchingRepository,
        ICurrentUserService currentUserService)
    {
        _matchingEngine =
            matchingEngine
            ?? throw new ArgumentNullException(
                nameof(matchingEngine));

        _matchingRepository =
            matchingRepository
            ?? throw new ArgumentNullException(
                nameof(matchingRepository));

        _currentUserService =
            currentUserService
            ?? throw new ArgumentNullException(
                nameof(currentUserService));
    }

    /// <summary>
    /// Loads job and candidate information from the database,
    /// then performs the complete matching calculation.
    /// </summary>
    public async Task<MatchingCalculationResult?>
        CalculateMatchAsync(
            Guid jobId,
            Guid jobSeekerProfileId,
            CancellationToken cancellationToken = default)
    {
        if (jobId == Guid.Empty)
        {
            throw new ArgumentException(
                "JobId is required.",
                nameof(jobId));
        }

        if (jobSeekerProfileId == Guid.Empty)
        {
            throw new ArgumentException(
                "JobSeekerProfileId is required.",
                nameof(jobSeekerProfileId));
        }

        if (_matchingRepository is null)
        {
            throw new InvalidOperationException(
                "Matching repository is not available.");
        }
        await EnsureJobAccessAsync(
            jobId,
            cancellationToken);

        var input =
            await _matchingRepository
                .GetMatchingCalculationInputAsync(
                    jobId,
                    jobSeekerProfileId,
                    cancellationToken);

        if (input is null)
        {
            return null;
        }

        return CalculateMatch(input);
    }

    /// <summary>
    /// Calculates a database-backed match and stores a
    /// transparent historical result with all score details.
    /// </summary>
    public async Task<MatchingCalculationResult?>
        CalculateAndSaveMatchAsync(
            Guid jobId,
            Guid jobSeekerProfileId,
            CancellationToken cancellationToken = default)
    {
        var result =
            await CalculateMatchAsync(
                jobId,
                jobSeekerProfileId,
                cancellationToken);

        if (result is null)
        {
            return null;
        }

        if (_matchingRepository is null)
        {
            throw new InvalidOperationException(
                "Matching repository is not available.");
        }

        await _matchingRepository.SaveMatchResultAsync(
            result,
            cancellationToken);

        return result;
    }

    /// <summary>
    /// Calculates one complete candidate-to-job match.
    ///
    /// Eligibility is checked separately from scoring.
    /// Even when a candidate is not eligible, the weighted
    /// score is still calculated for explanation purposes.
    /// </summary>
    public MatchingCalculationResult CalculateMatch(
        MatchingCalculationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.JobId == Guid.Empty)
        {
            throw new ArgumentException(
                "JobId is required.",
                nameof(input.JobId));
        }

        if (input.JobSeekerProfileId == Guid.Empty)
        {
            throw new ArgumentException(
                "JobSeekerProfileId is required.",
                nameof(input.JobSeekerProfileId));
        }

        // ----------------------------------------------------
        // 1. ELIGIBILITY
        // ----------------------------------------------------
        var eligibilityResult =
            _matchingEngine.EvaluateEligibility(
                input.Eligibility);

        // ----------------------------------------------------
        // 2. CATEGORY SCORES
        // ----------------------------------------------------
        var skillsScore =
            _matchingEngine.CalculateSkills(
                input.Skills);

        var experienceScore =
            _matchingEngine.CalculateExperience(
                input.Experience);

        var educationScore =
            _matchingEngine.CalculateEducation(
                input.Education);

        var certificationScore =
            _matchingEngine.CalculateCertification(
                input.Certification);

        var locationScore =
            _matchingEngine.CalculateLocation(
                input.Location);

        var projectsScore =
            _matchingEngine.CalculateProjects(
                input.Projects);

        var profileCompletionScore =
            _matchingEngine.CalculateProfileCompletion(
                input.ProfileCompletion);

        // ----------------------------------------------------
        // 3. WEIGHTED SCORE
        // ----------------------------------------------------
        var weightedResult =
            _matchingEngine.CalculateWeightedScore(
                new MatchScoreCalculationInput
                {
                    Skills = skillsScore,

                    Experience =
                        experienceScore,

                    Education =
                        educationScore,

                    Certification =
                        certificationScore,

                    Location =
                        locationScore,

                    Projects =
                        projectsScore,

                    ProfileCompletion =
                        profileCompletionScore,

                    SkillsWeight =
                        input.SkillsWeight,

                    ExperienceWeight =
                        input.ExperienceWeight,

                    EducationWeight =
                        input.EducationWeight,

                    CertificationWeight =
                        input.CertificationWeight,

                    LocationWeight =
                        input.LocationWeight,

                    ProjectsWeight =
                        input.ProjectsWeight,

                    ProfileCompletionWeight =
                        input.ProfileCompletionWeight
                });

        // ----------------------------------------------------
        // 4. RECOMMENDATION
        // ----------------------------------------------------
        var recommendation =
            _matchingEngine.GetRecommendation(
                weightedResult.TotalScore,
                eligibilityResult.IsEligible);

        // ----------------------------------------------------
        // 5. STRENGTHS
        // ----------------------------------------------------
        var strengths =
            weightedResult.ScoreDetails
                .Where(detail =>
                    detail.Status == "Strong" ||
                    detail.Status == "Good")
                .Select(detail =>
                    detail.Category)
                .ToList();

        // ----------------------------------------------------
        // 6. IMPROVEMENT AREAS
        // ----------------------------------------------------
        var improvementAreas =
            weightedResult.ScoreDetails
                .Where(detail =>
                    detail.Status == "Partial" ||
                    detail.Status ==
                        "Needs Improvement")
                .Select(detail =>
                    detail.Category)
                .ToList();

        // ----------------------------------------------------
        // 7. SUMMARY
        // ----------------------------------------------------
        var summary =
            BuildSummary(
                eligibilityResult.IsEligible,
                weightedResult.TotalScore,
                recommendation);

        // ----------------------------------------------------
        // 8. FINAL RESULT
        // ----------------------------------------------------
        return new MatchingCalculationResult
        {
            JobSeekerProfileId =
                input.JobSeekerProfileId,

            JobId =
                input.JobId,

            TotalScore =
                weightedResult.TotalScore,

            IsEligible =
                eligibilityResult.IsEligible,

            Recommendation =
                recommendation,

            Summary =
                summary,

            Strengths =
                strengths,

            ImprovementAreas =
                improvementAreas,

            MatchedMandatorySkills =
                eligibilityResult
                    .MatchedMandatorySkills
                    .ToList(),

            MissingMandatorySkills =
                eligibilityResult
                    .MissingMandatorySkills
                    .ToList(),

            EligibilityFailures =
                eligibilityResult
                    .FailureReasons
                    .ToList(),

            ScoreDetails =
                weightedResult.ScoreDetails
                    .ToList()
        };
    }

    /// <summary>
    /// Explains the current matching decision for one
    /// job application using the complete matching engine.
    /// </summary>
    public async Task<ExplainMatchDto?> ExplainMatchAsync(
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        if (applicationId == Guid.Empty)
        {
            throw new ArgumentException(
                "ApplicationId is required.",
                nameof(applicationId));
        }

        if (_matchingRepository is null)
        {
            throw new InvalidOperationException(
                "Matching repository is not available.");
        }

        var target =
            await _matchingRepository
                .GetApplicationMatchingTargetAsync(
                    applicationId,
                    cancellationToken);

        if (target is null)
        {
            return null;
        }

        var result =
            await CalculateMatchAsync(
                target.Value.JobId,
                target.Value.JobSeekerProfileId,
                cancellationToken);

        if (result is null)
        {
            return null;
        }

        return MatchingDtoMapper.ToExplainMatchDto(
            applicationId,
            result);
    }

    /// <summary>
    /// Loads active job applications and their latest saved
    /// eligible match result, then applies competition ranking.
    /// </summary>
    public async Task<IReadOnlyList<CandidateRankingResult>>
        GetRankedCandidatesForJobAsync(
            Guid jobId,
            int top = 10,
            CancellationToken cancellationToken = default)
    {
        if (jobId == Guid.Empty)
        {
            throw new ArgumentException(
                "JobId is required.",
                nameof(jobId));
        }

        if (top < 1 || top > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(top),
                "Top must be between 1 and 100.");
        }

        if (_matchingRepository is null)
        {
            throw new InvalidOperationException(
                "Matching repository is not available.");
        }

        await EnsureJobAccessAsync(
            jobId,
            cancellationToken);

        var inputs =
            await _matchingRepository
                .GetRankingInputsForJobAsync(
                    jobId,
                    cancellationToken);

        return RankCandidates(inputs)
            .Take(top)
            .ToList();
    }

    /// <summary>
    /// Ranks candidates using the matching engine.
    /// </summary>
    public List<CandidateRankingResult>
        RankCandidates(
            IEnumerable<CandidateRankingInput> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        return _matchingEngine.RankCandidates(
            candidates);
    }

    /// <summary>
    /// Compares between two and four selected active
    /// applications using their latest saved match results.
    /// </summary>
    public async Task<IReadOnlyList<CandidateComparisonResult>>
        CompareCandidatesForJobAsync(
            Guid jobId,
            IReadOnlyCollection<Guid> applicationIds,
            CancellationToken cancellationToken = default)
    {
        if (jobId == Guid.Empty)
        {
            throw new ArgumentException(
                "JobId is required.",
                nameof(jobId));
        }

        ArgumentNullException.ThrowIfNull(
            applicationIds);

        if (applicationIds.Count < 2 ||
            applicationIds.Count > 4)
        {
            throw new ArgumentException(
                "Between 2 and 4 applications must be selected.",
                nameof(applicationIds));
        }

        var uniqueIds =
            applicationIds
                .Distinct()
                .ToList();

        if (uniqueIds.Count != applicationIds.Count)
        {
            throw new ArgumentException(
                "The same application cannot be included more than once.",
                nameof(applicationIds));
        }

        if (_matchingRepository is null)
        {
            throw new InvalidOperationException(
                "Matching repository is not available.");
        }

        await EnsureJobAccessAsync(
            jobId,
            cancellationToken);

        var inputs =
            await _matchingRepository
                .GetComparisonInputsForJobAsync(
                    jobId,
                    uniqueIds,
                    cancellationToken);

        if (inputs.Count != uniqueIds.Count)
        {
            throw new InvalidOperationException(
                "One or more selected applications could not be compared. " +
                "They may be rejected, withdrawn, belong to another job, " +
                "or may not have a saved matching result.");
        }

        return CompareCandidates(inputs);
    }

    /// <summary>
    /// Compares between two and four candidates.
    /// </summary>
    public List<CandidateComparisonResult>
        CompareCandidates(
            IEnumerable<CandidateComparisonInput> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        return _matchingEngine.CompareCandidates(
            candidates);
    }

    /// <summary>
    /// Returns the currently active matching weight configuration.
    /// </summary>
    public async Task<MatchingRuleDto?> GetActiveMatchingRuleAsync(
        CancellationToken cancellationToken = default)
    {
        if (_matchingRepository is null)
        {
            throw new InvalidOperationException(
                "Matching repository is not available.");
        }

        var rule =
            await _matchingRepository
                .GetActiveMatchingRuleAsync(
                    cancellationToken);

        return rule is null
            ? null
            : MapMatchingRule(rule);
    }

    /// <summary>
    /// Replaces the active seven-category matching rule.
    /// No production default weights are invented.
    /// </summary>
    public async Task<MatchingRuleDto> ReplaceActiveMatchingRuleAsync(
        UpdateMatchingRuleRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateWeight(
            nameof(request.SkillsWeight),
            request.SkillsWeight);

        ValidateWeight(
            nameof(request.ExperienceWeight),
            request.ExperienceWeight);

        ValidateWeight(
            nameof(request.EducationWeight),
            request.EducationWeight);

        ValidateWeight(
            nameof(request.CertificationWeight),
            request.CertificationWeight);

        ValidateWeight(
            nameof(request.LocationWeight),
            request.LocationWeight);

        ValidateWeight(
            nameof(request.ProjectsWeight),
            request.ProjectsWeight);

        ValidateWeight(
            nameof(request.ProfileCompletionWeight),
            request.ProfileCompletionWeight);

        var total =
            request.SkillsWeight +
            request.ExperienceWeight +
            request.EducationWeight +
            request.CertificationWeight +
            request.LocationWeight +
            request.ProjectsWeight +
            request.ProfileCompletionWeight;

        if (total != 100m)
        {
            throw new ArgumentException(
                $"Matching rule weights must total exactly 100. " +
                $"Current total: {total:0.##}.");
        }

        if (_matchingRepository is null)
        {
            throw new InvalidOperationException(
                "Matching repository is not available.");
        }

        var rule =
            new NexHire.Domain.Entities.MatchingRule
            {
                Id = Guid.NewGuid(),

                SkillsWeight =
                    request.SkillsWeight,

                ExperienceWeight =
                    request.ExperienceWeight,

                EducationWeight =
                    request.EducationWeight,

                CertificationWeight =
                    request.CertificationWeight,

                LocationWeight =
                    request.LocationWeight,

                ProjectsWeight =
                    request.ProjectsWeight,

                ProfileCompletionWeight =
                    request.ProfileCompletionWeight,

                IsActive = true,

                CreatedAtUtc =
                    DateTime.UtcNow
            };

        var saved =
            await _matchingRepository
                .ReplaceActiveMatchingRuleAsync(
                    rule,
                    cancellationToken);

        return MapMatchingRule(saved);
    }

    private static MatchingRuleDto MapMatchingRule(
        NexHire.Domain.Entities.MatchingRule rule)
    {
        return new MatchingRuleDto
        {
            Id = rule.Id,

            SkillsWeight =
                rule.SkillsWeight,

            ExperienceWeight =
                rule.ExperienceWeight,

            EducationWeight =
                rule.EducationWeight,

            CertificationWeight =
                rule.CertificationWeight,

            LocationWeight =
                rule.LocationWeight,

            ProjectsWeight =
                rule.ProjectsWeight,

            ProfileCompletionWeight =
                rule.ProfileCompletionWeight,

            TotalWeight =
                rule.TotalWeight,

            IsActive =
                rule.IsActive,

            CreatedAtUtc =
                rule.CreatedAtUtc
        };
    }

    private static void ValidateWeight(
        string name,
        decimal value)
    {
        if (value < 0m || value > 100m)
        {
            throw new ArgumentOutOfRangeException(
                name,
                "Matching weights must be between 0 and 100.");
        }
    }

    /// <summary>
    /// Admins may access every job. Employers may only access
    /// matching data for jobs owned by their own company.
    /// </summary>
    private async Task EnsureJobAccessAsync(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        if (_currentUserService is null ||
            _matchingRepository is null)
        {
            throw new InvalidOperationException(
                "Matching authorization services are not available.");
        }

        if (_currentUserService.IsInRole("Admin"))
        {
            return;
        }

        if (!_currentUserService.IsInRole("Employer"))
        {
            throw new UnauthorizedAccessException(
                "You are not allowed to access employer matching data.");
        }

        var ownsJob =
            await _matchingRepository
                .EmployerOwnsJobAsync(
                    jobId,
                    _currentUserService.UserId,
                    cancellationToken);

        if (!ownsJob)
        {
            throw new UnauthorizedAccessException(
                "You are not allowed to access matching data for this job.");
        }
    }

    /// <summary>
    /// Creates a short readable explanation
    /// of the candidate match.
    /// </summary>
    private static string BuildSummary(
        bool isEligible,
        decimal totalScore,
        string recommendation)
    {
        if (!isEligible)
        {
            return
                $"Candidate is not eligible because one or more " +
                $"mandatory requirements were not met. " +
                $"Weighted match score: {totalScore:0.##}%. " +
                $"Recommendation: {recommendation}.";
        }

        return
            $"Candidate is eligible with an overall match score " +
            $"of {totalScore:0.##}%. " +
            $"Recommendation: {recommendation}.";
    }
}

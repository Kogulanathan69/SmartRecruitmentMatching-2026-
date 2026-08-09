using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Matching;

namespace NexHire.Application.Services;

/// <summary>
/// Coordinates the complete candidate-to-job matching process.
/// </summary>
public class MatchingService : IMatchingService
{
    private readonly IMatchingEngine _matchingEngine;
    private readonly IMatchingRepository? _matchingRepository;

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
        IMatchingRepository matchingRepository)
    {
        _matchingEngine =
            matchingEngine
            ?? throw new ArgumentNullException(
                nameof(matchingEngine));

        _matchingRepository =
            matchingRepository
            ?? throw new ArgumentNullException(
                nameof(matchingRepository));
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

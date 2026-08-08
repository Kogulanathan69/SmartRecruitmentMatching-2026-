using NexHire.Application.Interfaces.Services;
using NexHire.Application.Matching;

namespace NexHire.Application.Services;

/// <summary>
/// Coordinates the complete candidate-to-job matching process.
///
/// This service does not calculate scores by itself.
/// It asks IMatchingEngine to run the individual
/// eligibility and scoring components.
/// </summary>
public class MatchingService : IMatchingService
{
    private readonly IMatchingEngine _matchingEngine;

    public MatchingService(IMatchingEngine matchingEngine)
    {
        _matchingEngine = matchingEngine;
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

        // 1. Mandatory eligibility rules.
        var eligibilityResult =
            _matchingEngine.EvaluateEligibility(
                input.Eligibility);

        // 2. Calculate all seven category scores.
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

        // 3. Apply the active matching-rule weights.
        var weightedResult =
            _matchingEngine.CalculateWeightedScore(
                new MatchScoreCalculationInput
                {
                    Skills = skillsScore,
                    Experience = experienceScore,
                    Education = educationScore,
                    Certification = certificationScore,
                    Location = locationScore,
                    Projects = projectsScore,
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

        // 4. Recommendation uses both score and eligibility.
        var recommendation =
            _matchingEngine.GetRecommendation(
                weightedResult.TotalScore,
                eligibilityResult.IsEligible);

        // 5. Build human-readable explanation groups.
        var strengths = weightedResult.ScoreDetails
            .Where(detail =>
                detail.Status == "Strong" ||
                detail.Status == "Good")
            .Select(detail => detail.Category)
            .ToList();

        var improvementAreas =
            weightedResult.ScoreDetails
                .Where(detail =>
                    detail.Status == "Partial" ||
                    detail.Status ==
                        "Needs Improvement")
                .Select(detail => detail.Category)
                .ToList();

        // 6. Produce a short overall explanation.
        var summary = BuildSummary(
            eligibilityResult.IsEligible,
            weightedResult.TotalScore,
            recommendation);

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
    /// Delegates candidate ranking to the matching engine.
    /// </summary>
    public List<CandidateRankingResult> RankCandidates(
        IEnumerable<CandidateRankingInput> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        return _matchingEngine.RankCandidates(
            candidates);
    }

    /// <summary>
    /// Delegates candidate comparison to the matching engine.
    /// </summary>
    public List<CandidateComparisonResult> CompareCandidates(
        IEnumerable<CandidateComparisonInput> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        return _matchingEngine.CompareCandidates(
            candidates);
    }

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
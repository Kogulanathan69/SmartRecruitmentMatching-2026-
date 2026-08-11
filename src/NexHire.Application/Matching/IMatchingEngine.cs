namespace NexHire.Application.Matching;

/// <summary>
/// Provides one clean entry point for the complete
/// matching engine.
///
/// MatchingService belongs to the Application layer and
/// must not directly reference Infrastructure classes.
///
/// The Infrastructure layer will implement this interface
/// and connect the individual matching calculators.
/// </summary>
public interface IMatchingEngine
{
    /// <summary>
    /// Checks the mandatory eligibility rules.
    /// </summary>
    EligibilityResult EvaluateEligibility(
        EligibilityInput input);

    /// <summary>
    /// Calculates the Skills category score.
    /// </summary>
    CategoryScoreResult CalculateSkills(
        SkillMatchInput input);

    /// <summary>
    /// Calculates the Experience category score.
    /// </summary>
    CategoryScoreResult CalculateExperience(
        ExperienceMatchInput input);

    /// <summary>
    /// Calculates the Education category score.
    /// </summary>
    CategoryScoreResult CalculateEducation(
        EducationMatchInput input);

    /// <summary>
    /// Calculates the Certification category score.
    /// </summary>
    CategoryScoreResult CalculateCertification(
        CertificationMatchInput input);

    /// <summary>
    /// Calculates the Location category score.
    /// </summary>
    CategoryScoreResult CalculateLocation(
        LocationMatchInput input);

    /// <summary>
    /// Calculates the Projects category score.
    /// </summary>
    CategoryScoreResult CalculateProjects(
        ProjectMatchInput input);

    /// <summary>
    /// Calculates the Profile Completion category score.
    /// </summary>
    CategoryScoreResult CalculateProfileCompletion(
        ProfileCompletionMatchInput input);

    /// <summary>
    /// Applies configured weights to all category scores
    /// and returns the final weighted score.
    /// </summary>
    MatchScoreCalculationResult CalculateWeightedScore(
        MatchScoreCalculationInput input);

    /// <summary>
    /// Converts eligibility and total score
    /// into a recommendation.
    /// </summary>
    string GetRecommendation(
        decimal totalScore,
        bool isEligible);

    /// <summary>
    /// Applies competition ranking such as
    /// 1, 2, 2, 4.
    /// </summary>
    List<CandidateRankingResult> RankCandidates(
        IEnumerable<CandidateRankingInput> candidates);

    /// <summary>
    /// Validates and prepares 2 to 4 candidates
    /// for side-by-side comparison.
    /// </summary>
    List<CandidateComparisonResult> CompareCandidates(
        IEnumerable<CandidateComparisonInput> candidates);
}
using NexHire.Application.Matching;

namespace NexHire.Infrastructure.Matching;

/// <summary>
/// Infrastructure implementation of the complete matching engine.
///
/// This class acts as one facade over the individual
/// eligibility, scoring, recommendation, ranking and
/// comparison components.
///
/// MatchingService can depend only on IMatchingEngine
/// without directly depending on Infrastructure classes.
/// </summary>
public class MatchingEngine : IMatchingEngine
{
    private readonly IEligibilityEngine _eligibilityEngine;
    private readonly SkillMatchCalculator _skillMatchCalculator;
    private readonly ExperienceMatchCalculator _experienceMatchCalculator;
    private readonly EducationMatchCalculator _educationMatchCalculator;
    private readonly CertificationMatchCalculator _certificationMatchCalculator;
    private readonly LocationMatchCalculator _locationMatchCalculator;
    private readonly ProjectMatchCalculator _projectMatchCalculator;
    private readonly ProfileCompletionMatchCalculator _profileCompletionMatchCalculator;
    private readonly MatchScoreCalculator _matchScoreCalculator;
    private readonly IRecommendationEngine _recommendationEngine;
    private readonly CandidateRankingEngine _candidateRankingEngine;
    private readonly CandidateComparisonEngine _candidateComparisonEngine;

    public MatchingEngine(
        IEligibilityEngine eligibilityEngine,
        SkillMatchCalculator skillMatchCalculator,
        ExperienceMatchCalculator experienceMatchCalculator,
        EducationMatchCalculator educationMatchCalculator,
        CertificationMatchCalculator certificationMatchCalculator,
        LocationMatchCalculator locationMatchCalculator,
        ProjectMatchCalculator projectMatchCalculator,
        ProfileCompletionMatchCalculator profileCompletionMatchCalculator,
        MatchScoreCalculator matchScoreCalculator,
        IRecommendationEngine recommendationEngine,
        CandidateRankingEngine candidateRankingEngine,
        CandidateComparisonEngine candidateComparisonEngine)
    {
        _eligibilityEngine = eligibilityEngine;
        _skillMatchCalculator = skillMatchCalculator;
        _experienceMatchCalculator = experienceMatchCalculator;
        _educationMatchCalculator = educationMatchCalculator;
        _certificationMatchCalculator = certificationMatchCalculator;
        _locationMatchCalculator = locationMatchCalculator;
        _projectMatchCalculator = projectMatchCalculator;
        _profileCompletionMatchCalculator = profileCompletionMatchCalculator;
        _matchScoreCalculator = matchScoreCalculator;
        _recommendationEngine = recommendationEngine;
        _candidateRankingEngine = candidateRankingEngine;
        _candidateComparisonEngine = candidateComparisonEngine;
    }

    public EligibilityResult EvaluateEligibility(
        EligibilityInput input)
    {
        return _eligibilityEngine.Evaluate(input);
    }

    public CategoryScoreResult CalculateSkills(
        SkillMatchInput input)
    {
        return _skillMatchCalculator.Calculate(input);
    }

    public CategoryScoreResult CalculateExperience(
        ExperienceMatchInput input)
    {
        return _experienceMatchCalculator.Calculate(input);
    }

    public CategoryScoreResult CalculateEducation(
        EducationMatchInput input)
    {
        return _educationMatchCalculator.Calculate(input);
    }

    public CategoryScoreResult CalculateCertification(
        CertificationMatchInput input)
    {
        return _certificationMatchCalculator.Calculate(input);
    }

    public CategoryScoreResult CalculateLocation(
        LocationMatchInput input)
    {
        return _locationMatchCalculator.Calculate(input);
    }

    public CategoryScoreResult CalculateProjects(
        ProjectMatchInput input)
    {
        return _projectMatchCalculator.Calculate(input);
    }

    public CategoryScoreResult CalculateProfileCompletion(
        ProfileCompletionMatchInput input)
    {
        return _profileCompletionMatchCalculator.Calculate(input);
    }

    public MatchScoreCalculationResult CalculateWeightedScore(
        MatchScoreCalculationInput input)
    {
        return _matchScoreCalculator.Calculate(input);
    }

    public string GetRecommendation(
        decimal totalScore,
        bool isEligible)
    {
        return _recommendationEngine.GetRecommendation(
            totalScore,
            isEligible);
    }

    public List<CandidateRankingResult> RankCandidates(
        IEnumerable<CandidateRankingInput> candidates)
    {
        return _candidateRankingEngine.Rank(candidates);
    }

    public List<CandidateComparisonResult> CompareCandidates(
        IEnumerable<CandidateComparisonInput> candidates)
    {
        return _candidateComparisonEngine.Compare(candidates);
    }
}
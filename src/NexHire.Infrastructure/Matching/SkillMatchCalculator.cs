using NexHire.Application.Common;
using NexHire.Application.Matching;

namespace NexHire.Infrastructure.Matching;

/// <summary>
/// Calculates the Skills category score.
///
/// The calculator works only with SkillMatchInput.
/// It does not directly depend on JobSkill or JobSeekerSkill entities,
/// so team entity changes can be handled in the mapping layer.
/// </summary>
public class SkillMatchCalculator
{
    public CategoryScoreResult Calculate(SkillMatchInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var hasMandatorySkills = input.MandatorySkills.Count > 0;
        var hasPreferredSkills = input.PreferredSkills.Count > 0;

        decimal rawScore;
        string explanation;

        // ---------------------------------------------------------
        // CASE 1: Mandatory + Preferred skills both exist
        // ---------------------------------------------------------
        if (hasMandatorySkills && hasPreferredSkills)
        {
            var mandatoryScore = CalculateRequirementScore(
                input.MandatorySkills,
                input.CandidateSkills);

            var preferredScore = CalculateRequirementScore(
                input.PreferredSkills,
                input.CandidateSkills);

            rawScore =
                (mandatoryScore * MatchingConstants.MandatorySkillShare) +
                (preferredScore * MatchingConstants.PreferredSkillShare);

            explanation =
                $"Mandatory skills score: {mandatoryScore:0.##}/100. " +
                $"Preferred skills score: {preferredScore:0.##}/100. " +
                "Mandatory skills contribute 80% and preferred skills contribute 20%.";
        }

        // ---------------------------------------------------------
        // CASE 2: Only mandatory skills exist
        // ---------------------------------------------------------
        else if (hasMandatorySkills)
        {
            rawScore = CalculateRequirementScore(
                input.MandatorySkills,
                input.CandidateSkills);

            explanation =
                $"Skills score is based on mandatory skills only: {rawScore:0.##}/100.";
        }

        // ---------------------------------------------------------
        // CASE 3: Only preferred skills exist
        // ---------------------------------------------------------
        else if (hasPreferredSkills)
        {
            rawScore = CalculateRequirementScore(
                input.PreferredSkills,
                input.CandidateSkills);

            explanation =
                $"Skills score is based on preferred skills only: {rawScore:0.##}/100.";
        }

        // ---------------------------------------------------------
        // CASE 4: Job has no skill requirements
        // ---------------------------------------------------------
        else
        {
            // There is no skill requirement for the candidate to fail.
            // This can be changed later in one place if the team decides
            // a different business rule for jobs without skill requirements.
            rawScore = MatchingConstants.MaximumScore;

            explanation =
                "No skill requirements are configured for this job.";
        }

        rawScore = Math.Clamp(
            rawScore,
            MatchingConstants.MinimumScore,
            MatchingConstants.MaximumScore);

        rawScore = Math.Round(rawScore, 2);

        return new CategoryScoreResult
        {
            RawScore = rawScore,
            Status = GetStatus(rawScore),
            Explanation = explanation
        };
    }

    /// <summary>
    /// Calculates how many required skills are fully satisfied.
    /// A skill counts as matched only when the candidate has the skill
    /// and meets the required minimum proficiency level.
    /// </summary>
    private static decimal CalculateRequirementScore(
        List<RequiredSkillInput> requirements,
        List<CandidateSkillInput> candidateSkills)
    {
        if (requirements.Count == 0)
        {
            return MatchingConstants.MaximumScore;
        }

        var matchedCount = 0;

        foreach (var requirement in requirements)
        {
            var requiredSkillName =
                (requirement.SkillName ?? string.Empty).Trim();

            CandidateSkillInput? matchedSkill = null;

            foreach (var candidateSkill in candidateSkills)
            {
                var candidateSkillName =
                    (candidateSkill.SkillName ?? string.Empty).Trim();

                if (string.Equals(
                        requiredSkillName,
                        candidateSkillName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    matchedSkill = candidateSkill;
                    break;
                }
            }

            if (matchedSkill is not null &&
                matchedSkill.ProficiencyLevel >=
                requirement.MinimumProficiencyLevel)
            {
                matchedCount++;
            }
        }

        return matchedCount * 100m / requirements.Count;
    }

    /// <summary>
    /// Converts the numeric score into a simple explanation status.
    ///
    /// The thresholds are based on the central matching score bands.
    /// If the team changes the bands later, we can update them centrally.
    /// </summary>
    private static string GetStatus(decimal score)
    {
        if (score >= MatchingConstants.HighlyRecommendedMinimum)
        {
            return "Strong";
        }

        if (score >= MatchingConstants.RecommendedForReviewMinimum)
        {
            return "Good";
        }

        if (score >= MatchingConstants.ConsiderWithCautionMinimum)
        {
            return "Partial";
        }

        return "Needs Improvement";
    }
}
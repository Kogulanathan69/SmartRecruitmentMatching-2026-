namespace NexHire.Application.Matching;

/// <summary>
/// Checks the mandatory eligibility requirements before
/// the weighted matching score is calculated.
///
/// This class contains only business logic.
/// It does not access the database directly.
/// </summary>
public class EligibilityEngine : IEligibilityEngine
{
    public EligibilityResult Evaluate(EligibilityInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var result = EligibilityResult.Eligible();

        // ---------------------------------------------------------
        // 1. JOB AVAILABILITY
        // ---------------------------------------------------------
        if (!input.IsJobAvailable)
        {
            result.FailureReasons.Add(
                string.IsNullOrWhiteSpace(input.JobUnavailableReason)
                    ? "The job is not currently available."
                    : input.JobUnavailableReason);
        }

        // ---------------------------------------------------------
        // 2. MANDATORY SKILLS
        // ---------------------------------------------------------
        foreach (var requiredSkill in input.MandatorySkills)
        {
            // Convert possible null values into safe non-null strings.
            // This prevents nullable warnings and makes integration safer
            // if another team member's mapping provides incomplete data.
            var requiredSkillName =
                (requiredSkill.SkillName ?? string.Empty).Trim();

            CandidateSkillInput? candidateSkill = null;

            foreach (var skill in input.CandidateSkills)
            {
                var candidateSkillName =
                    (skill.SkillName ?? string.Empty).Trim();

                if (string.Equals(
                        candidateSkillName,
                        requiredSkillName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    candidateSkill = skill;
                    break;
                }
            }

            // Candidate does not have this mandatory skill.
            if (candidateSkill is null)
            {
                result.MissingMandatorySkills.Add(requiredSkillName);

                result.FailureReasons.Add(
                    $"Mandatory skill '{requiredSkillName}' is missing.");

                continue;
            }

            // Candidate has the skill, but proficiency is too low.
            if (candidateSkill.ProficiencyLevel <
                requiredSkill.MinimumProficiencyLevel)
            {
                result.MissingMandatorySkills.Add(requiredSkillName);

                result.FailureReasons.Add(
                    $"Mandatory skill '{requiredSkillName}' does not meet the required proficiency level.");

                continue;
            }

            // Mandatory skill requirement is fully satisfied.
            result.MatchedMandatorySkills.Add(requiredSkillName);
        }

        // ---------------------------------------------------------
        // 3. EXPERIENCE
        // ---------------------------------------------------------
        if (input.CandidateExperienceYears <
            input.MinimumExperienceYears)
        {
            result.FailureReasons.Add(
                $"Minimum experience requirement is not met. Required: {input.MinimumExperienceYears} year(s), Candidate: {input.CandidateExperienceYears} year(s).");
        }

        // ---------------------------------------------------------
        // 4. EDUCATION
        // ---------------------------------------------------------
        if (input.CandidateEducationLevel <
            input.MinimumEducationLevel)
        {
            result.FailureReasons.Add(
                "Minimum education requirement is not met.");
        }

        // ---------------------------------------------------------
        // FINAL RESULT
        // ---------------------------------------------------------
        result.IsEligible = result.FailureReasons.Count == 0;

        return result;
    }
}
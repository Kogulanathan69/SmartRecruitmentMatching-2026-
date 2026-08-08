namespace NexHire.Application.Matching;

/// <summary>
/// Contains only the skill information required by the
/// skill matching calculator.
///
/// The calculator does not depend directly on the Job,
/// JobSkill or JobSeekerSkill database entities.
/// This allows team entity models to change without
/// rewriting the matching calculation.
/// </summary>
public class SkillMatchInput
{
    /// <summary>
    /// Skills that the candidate must satisfy.
    /// </summary>
    public List<RequiredSkillInput> MandatorySkills { get; set; } = new();

    /// <summary>
    /// Extra skills preferred by the employer.
    /// These improve the match score but do not decide eligibility.
    /// </summary>
    public List<RequiredSkillInput> PreferredSkills { get; set; } = new();

    /// <summary>
    /// Skills available in the candidate profile.
    /// </summary>
    public List<CandidateSkillInput> CandidateSkills { get; set; } = new();
}
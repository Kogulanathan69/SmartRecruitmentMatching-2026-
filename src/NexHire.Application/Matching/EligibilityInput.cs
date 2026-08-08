namespace NexHire.Application.Matching;

/// <summary>
/// Contains only the information required by the eligibility engine.
///
/// Important:
/// This class keeps the matching logic independent from the database
/// entities created by other team members.
///
/// If Job, CandidateSkill, Education or Experience entities change later,
/// we only need to update the mapping code instead of rewriting the engine.
/// </summary>
public class EligibilityInput
{
    public bool IsJobAvailable { get; set; }

    public string JobUnavailableReason { get; set; } = string.Empty;

    public decimal CandidateExperienceYears { get; set; }

    public decimal MinimumExperienceYears { get; set; }

    public int CandidateEducationLevel { get; set; }

    public int MinimumEducationLevel { get; set; }

    public List<RequiredSkillInput> MandatorySkills { get; set; } = new();

    public List<CandidateSkillInput> CandidateSkills { get; set; } = new();
}

/// <summary>
/// Describes one mandatory skill required by the job.
/// </summary>
public class RequiredSkillInput
{
    public string SkillName { get; set; } = string.Empty;

    public decimal MinimumProficiencyLevel { get; set; }
}

/// <summary>
/// Describes one skill currently owned by the candidate.
/// </summary>
public class CandidateSkillInput
{
    public string SkillName { get; set; } = string.Empty;

    public decimal ProficiencyLevel { get; set; }
}
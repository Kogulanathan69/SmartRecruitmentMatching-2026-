namespace NexHire.Application.Matching;

/// <summary>
/// Contains only the experience information required
/// by the experience matching calculator.
///
/// This class is intentionally independent from the
/// JobSeekerExperience and Job database entities.
///
/// If another team member changes their entity property names later,
/// only the mapping layer needs to change.
/// </summary>
public class ExperienceMatchInput
{
    /// <summary>
    /// Total relevant experience owned by the candidate.
    /// </summary>
    public decimal CandidateExperienceYears { get; set; }

    /// <summary>
    /// Minimum experience required by the job.
    /// </summary>
    public decimal RequiredExperienceYears { get; set; }
}
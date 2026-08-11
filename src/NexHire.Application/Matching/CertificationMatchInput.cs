namespace NexHire.Application.Matching;

/// <summary>
/// Contains only the certification information required
/// by the certification matching calculator.
///
/// This model is kept independent from the database entities
/// created by other team members.
///
/// If the team's certification models change later,
/// only the mapping code needs to change.
/// </summary>
public class CertificationMatchInput
{
    /// <summary>
    /// Certifications requested or preferred for the job.
    /// </summary>
    public List<string> RequiredCertifications { get; set; } = new();

    /// <summary>
    /// Certifications currently owned by the candidate.
    /// </summary>
    public List<string> CandidateCertifications { get; set; } = new();
}
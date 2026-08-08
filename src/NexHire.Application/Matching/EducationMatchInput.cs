namespace NexHire.Application.Matching;

/// <summary>
/// Contains only the education information required
/// by the education matching calculator.
///
/// This model keeps the matching logic independent from
/// the education entities created by other team members.
///
/// If the team's Education model changes later,
/// only the mapping layer should need to change.
/// </summary>
public class EducationMatchInput
{
    /// <summary>
    /// Candidate's highest education level represented
    /// using an ordered numeric value.
    ///
    /// Example:
    /// 1 = basic level
    /// 2 = diploma level
    /// 3 = degree level
    ///
    /// The final mapping will be connected to the team's
    /// actual education model during integration.
    /// </summary>
    public int CandidateEducationLevel { get; set; }

    /// <summary>
    /// Minimum education level required by the job.
    /// Uses the same ordered scale as CandidateEducationLevel.
    /// </summary>
    public int RequiredEducationLevel { get; set; }
}
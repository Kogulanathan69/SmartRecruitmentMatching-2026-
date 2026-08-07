namespace NexHire.Application.Matching;

/// <summary>
/// Contains the candidate profile completion value
/// required by the matching system.
///
/// The actual completion percentage can later be calculated
/// from the final Job Seeker profile fields created by the team.
///
/// Keeping that calculation outside this class prevents the
/// matching engine from depending on changing profile entities.
/// </summary>
public class ProfileCompletionMatchInput
{
    /// <summary>
    /// Candidate profile completion percentage.
    ///
    /// Expected range: 0 to 100.
    ///
    /// Example:
    /// 100 = fully completed profile
    /// 75  = mostly completed profile
    /// 50  = partially completed profile
    /// </summary>
    public decimal CompletionPercentage { get; set; }
}
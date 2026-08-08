namespace NexHire.Application.Matching;

/// <summary>
/// Defines the contract for checking whether a candidate is eligible
/// for a job before the weighted matching score is calculated.
///
/// The engine receives EligibilityInput instead of database entities.
/// This keeps the matching logic independent from other team members'
/// Job, Skill, Experience and Education entity implementations.
/// </summary>
public interface IEligibilityEngine
{
    /// <summary>
    /// Checks the mandatory job and candidate requirements.
    /// </summary>
    /// <param name="input">
    /// Simple eligibility data prepared by the service/repository layer.
    /// </param>
    /// <returns>
    /// Eligibility result containing eligibility status,
    /// failure reasons and mandatory skill evidence.
    /// </returns>
    EligibilityResult Evaluate(EligibilityInput input);
}
using NexHire.Application.Common;
using NexHire.Application.Matching;

namespace NexHire.Infrastructure.Matching;

/// <summary>
/// Prepares candidates for side-by-side comparison.
///
/// The engine validates the allowed comparison count,
/// prevents duplicate applications and orders candidates
/// by their final matching score.
/// </summary>
public class CandidateComparisonEngine
{
    /// <summary>
    /// Compares between 2 and 4 unique candidate applications.
    ///
    /// Candidates are returned from highest matching score
    /// to lowest matching score.
    ///
    /// When scores are equal, ApplicationId is used only
    /// to keep the result order deterministic.
    /// </summary>
    public List<CandidateComparisonResult> Compare(
        IEnumerable<CandidateComparisonInput> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        var candidateList = candidates.ToList();

        // ---------------------------------------------------------
        // VALIDATE NUMBER OF CANDIDATES
        // ---------------------------------------------------------
        if (candidateList.Count <
            MatchingConstants.MinimumCandidatesToCompare)
        {
            throw new ArgumentException(
                $"At least {MatchingConstants.MinimumCandidatesToCompare} " +
                "candidates are required for comparison.",
                nameof(candidates));
        }

        if (candidateList.Count >
            MatchingConstants.MaximumCandidatesToCompare)
        {
            throw new ArgumentException(
                $"A maximum of {MatchingConstants.MaximumCandidatesToCompare} " +
                "candidates can be compared at one time.",
                nameof(candidates));
        }

        // ---------------------------------------------------------
        // PREVENT THE SAME APPLICATION FROM BEING COMPARED TWICE
        // ---------------------------------------------------------
        var uniqueApplicationCount = candidateList
            .Select(candidate => candidate.ApplicationId)
            .Distinct()
            .Count();

        if (uniqueApplicationCount != candidateList.Count)
        {
            throw new ArgumentException(
                "The same application cannot be included more than once.",
                nameof(candidates));
        }

        // ---------------------------------------------------------
        // NORMALIZE AND ORDER
        // ---------------------------------------------------------
        return candidateList
            .Select(candidate =>
            {
                var totalScore = Math.Round(
                    Math.Clamp(
                        candidate.TotalScore,
                        MatchingConstants.MinimumScore,
                        MatchingConstants.MaximumScore),
                    2);

                return new CandidateComparisonResult
                {
                    ApplicationId = candidate.ApplicationId,
                    JobSeekerProfileId =
                        candidate.JobSeekerProfileId,

                    TotalScore = totalScore,
                    IsEligible = candidate.IsEligible,
                    Recommendation =
                        candidate.Recommendation,

                    // Create a new list so the result does not
                    // reuse the input list instance directly.
                    ScoreDetails =
                        candidate.ScoreDetails.ToList()
                };
            })
            .OrderByDescending(candidate =>
                candidate.TotalScore)

            // Tie scores keep a predictable result order.
            // This does not change the matching score.
            .ThenBy(candidate =>
                candidate.ApplicationId)

            .ToList();
    }
}
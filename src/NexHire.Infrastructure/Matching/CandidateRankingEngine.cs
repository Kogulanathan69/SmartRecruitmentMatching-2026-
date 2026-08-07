using NexHire.Application.Common;
using NexHire.Application.Matching;

namespace NexHire.Infrastructure.Matching;

/// <summary>
/// Ranks candidate applications using their final
/// weighted matching scores.
///
/// This engine uses competition ranking.
///
/// Example:
/// Scores: 95, 90, 90, 80
/// Ranks : 1,  2,  2,  4
///
/// Filtering application statuses such as Rejected
/// or Withdrawn is intentionally handled before this
/// engine in the service/repository layer.
/// </summary>
public class CandidateRankingEngine
{
    /// <summary>
    /// Ranks candidates from highest score to lowest score
    /// and marks candidates that share the same score as tied.
    ///
    /// Competition ranking rule:
    /// 95 -> Rank 1
    /// 90 -> Rank 2
    /// 90 -> Rank 2
    /// 80 -> Rank 4
    /// </summary>
    public List<CandidateRankingResult> Rank(
        IEnumerable<CandidateRankingInput> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        // ---------------------------------------------------------
        // NORMALIZE AND SORT
        // ---------------------------------------------------------
        var orderedCandidates = candidates
            .Select(candidate => new CandidateRankingInput
            {
                ApplicationId = candidate.ApplicationId,
                JobSeekerProfileId = candidate.JobSeekerProfileId,

                TotalScore = Math.Round(
                    Math.Clamp(
                        candidate.TotalScore,
                        MatchingConstants.MinimumScore,
                        MatchingConstants.MaximumScore),
                    2),

                IsEligible = candidate.IsEligible,
                Recommendation = candidate.Recommendation
            })
            .OrderByDescending(candidate => candidate.TotalScore)

            // This does NOT affect rank.
            // It only gives deterministic ordering when scores tie.
            .ThenBy(candidate => candidate.ApplicationId)
            .ToList();

        var results = new List<CandidateRankingResult>();

        if (orderedCandidates.Count == 0)
        {
            return results;
        }

        // ---------------------------------------------------------
        // DETECT HOW MANY CANDIDATES SHARE EACH SCORE
        // ---------------------------------------------------------
        var scoreCounts = orderedCandidates
            .GroupBy(candidate => candidate.TotalScore)
            .ToDictionary(
                group => group.Key,
                group => group.Count());

        decimal? previousScore = null;
        var currentRank = 0;

        // ---------------------------------------------------------
        // APPLY COMPETITION RANKING
        // ---------------------------------------------------------
        for (var index = 0; index < orderedCandidates.Count; index++)
        {
            var candidate = orderedCandidates[index];

            // A new score receives its actual list position as rank.
            // Same score keeps the previous candidate's rank.
            if (previousScore is null ||
                candidate.TotalScore != previousScore.Value)
            {
                currentRank = index + 1;
            }

            var isTied =
                scoreCounts[candidate.TotalScore] > 1;

            results.Add(new CandidateRankingResult
            {
                ApplicationId = candidate.ApplicationId,
                JobSeekerProfileId = candidate.JobSeekerProfileId,
                Rank = currentRank,
                TotalScore = candidate.TotalScore,
                IsEligible = candidate.IsEligible,
                IsTied = isTied,
                Recommendation = candidate.Recommendation
            });

            previousScore = candidate.TotalScore;
        }

        return results;
    }
}
using NexHire.Application.Common;
using NexHire.Application.Matching;

namespace NexHire.Infrastructure.Matching;

/// <summary>
/// Calculates the Location category score.
///
/// This calculator compares the candidate location
/// with the job location using a simple deterministic rule.
///
/// The calculator is independent from database entities,
/// so team location-model changes can later be handled
/// only in the mapping layer.
/// </summary>
public class LocationMatchCalculator
{
    /// <summary>
    /// Calculates a location score from 0 to 100.
    ///
    /// Rule:
    /// - No job location requirement = 100.
    /// - Candidate location matches job location = 100.
    /// - Candidate location is different = 0.
    ///
    /// Location comparison ignores uppercase/lowercase
    /// and extra spaces around the text.
    /// </summary>
    public CategoryScoreResult Calculate(LocationMatchInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var candidateLocation =
            NormalizeLocation(input.CandidateLocation);

        var jobLocation =
            NormalizeLocation(input.JobLocation);

        decimal rawScore;
        string explanation;

        // ---------------------------------------------------------
        // CASE 1: JOB HAS NO LOCATION REQUIREMENT
        // ---------------------------------------------------------
        if (string.IsNullOrWhiteSpace(jobLocation))
        {
            rawScore = MatchingConstants.MaximumScore;

            explanation =
                "This job does not have a specific location requirement.";
        }

        // ---------------------------------------------------------
        // CASE 2: CANDIDATE LOCATION MATCHES JOB LOCATION
        // ---------------------------------------------------------
        else if (string.Equals(
                     candidateLocation,
                     jobLocation,
                     StringComparison.OrdinalIgnoreCase))
        {
            rawScore = MatchingConstants.MaximumScore;

            explanation =
                $"Candidate location matches the job location ({jobLocation}).";
        }

        // ---------------------------------------------------------
        // CASE 3: LOCATIONS DO NOT MATCH
        // ---------------------------------------------------------
        else
        {
            rawScore = MatchingConstants.MinimumScore;

            explanation =
                $"Candidate location ({DisplayLocation(candidateLocation)}) " +
                $"does not match the job location ({jobLocation}).";
        }

        return new CategoryScoreResult
        {
            RawScore = rawScore,
            Status = GetStatus(rawScore),
            Explanation = explanation
        };
    }

    /// <summary>
    /// Removes unnecessary spaces from location text.
    /// </summary>
    private static string NormalizeLocation(string? location)
    {
        return (location ?? string.Empty).Trim();
    }

    /// <summary>
    /// Provides readable text when the candidate
    /// has not provided a location.
    /// </summary>
    private static string DisplayLocation(string location)
    {
        return string.IsNullOrWhiteSpace(location)
            ? "Not provided"
            : location;
    }

    /// <summary>
    /// Converts the numeric score into the common
    /// explainable matching status.
    /// </summary>
    private static string GetStatus(decimal score)
    {
        if (score >= MatchingConstants.HighlyRecommendedMinimum)
        {
            return "Strong";
        }

        if (score >= MatchingConstants.RecommendedForReviewMinimum)
        {
            return "Good";
        }

        if (score >= MatchingConstants.ConsiderWithCautionMinimum)
        {
            return "Partial";
        }

        return "Needs Improvement";
    }
}
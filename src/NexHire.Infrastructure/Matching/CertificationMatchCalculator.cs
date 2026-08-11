using NexHire.Application.Common;
using NexHire.Application.Matching;

namespace NexHire.Infrastructure.Matching;

/// <summary>
/// Calculates the Certification category score.
///
/// The calculator works only with simple certification names,
/// so it does not depend directly on database entities.
///
/// Team entity changes can later be handled in the mapping layer
/// without rewriting this matching logic.
/// </summary>
public class CertificationMatchCalculator
{
    /// <summary>
    /// Calculates how many of the job's required certifications
    /// are owned by the candidate.
    ///
    /// Formula:
    /// matched required certifications / total required certifications * 100
    ///
    /// If the job has no certification requirement,
    /// the candidate receives the full category score.
    /// </summary>
    public CategoryScoreResult Calculate(CertificationMatchInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var requiredCertifications = NormalizeCertifications(
            input.RequiredCertifications);

        var candidateCertifications = NormalizeCertifications(
            input.CandidateCertifications);

        decimal rawScore;
        string explanation;

        // ---------------------------------------------------------
        // CASE 1: JOB HAS NO CERTIFICATION REQUIREMENT
        // ---------------------------------------------------------
        if (requiredCertifications.Count == 0)
        {
            rawScore = MatchingConstants.MaximumScore;

            explanation =
                "This job does not require any certifications.";
        }
        else
        {
            // -----------------------------------------------------
            // COUNT REQUIRED CERTIFICATIONS OWNED BY THE CANDIDATE
            // -----------------------------------------------------
            var matchedCount = requiredCertifications.Count(
                requiredCertification =>
                    candidateCertifications.Contains(
                        requiredCertification,
                        StringComparer.OrdinalIgnoreCase));

            rawScore =
                (decimal)matchedCount /
                requiredCertifications.Count *
                MatchingConstants.MaximumScore;

            rawScore = Math.Clamp(
                rawScore,
                MatchingConstants.MinimumScore,
                MatchingConstants.MaximumScore);

            rawScore = Math.Round(rawScore, 2);

            explanation =
                $"Candidate matches {matchedCount} of " +
                $"{requiredCertifications.Count} required certification(s).";
        }

        return new CategoryScoreResult
        {
            RawScore = rawScore,
            Status = GetStatus(rawScore),
            Explanation = explanation
        };
    }

    /// <summary>
    /// Removes blank values, extra spaces and duplicate
    /// certification names before score calculation.
    ///
    /// Certification names are compared without considering
    /// uppercase or lowercase differences.
    /// </summary>
    private static List<string> NormalizeCertifications(
        IEnumerable<string>? certifications)
    {
        if (certifications is null)
        {
            return new List<string>();
        }

        return certifications
            .Where(certification =>
                !string.IsNullOrWhiteSpace(certification))
            .Select(certification => certification.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
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
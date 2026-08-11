namespace NexHire.Application.Matching;

/// <summary>
/// Represents the calculated result for one matching category,
/// such as Skills, Experience, Education or Location.
///
/// All score calculators return the same simple structure.
/// This makes the scoring engine easier to combine, test and
/// update if another team member changes their entity models later.
/// </summary>
public class CategoryScoreResult
{
    /// <summary>
    /// Category score before its configured weight is applied.
    /// The value must stay between 0 and 100.
    /// </summary>
    public decimal RawScore { get; set; }

    /// <summary>
    /// Human-readable level such as:
    /// Strong, Good, Partial or Needs Improvement.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Simple explanation showing why this score was given.
    /// This will later be shown in the explainable matching response.
    /// </summary>
    public string Explanation { get; set; } = string.Empty;
}
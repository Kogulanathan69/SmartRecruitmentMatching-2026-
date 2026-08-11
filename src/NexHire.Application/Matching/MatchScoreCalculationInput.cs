namespace NexHire.Application.Matching;

/// <summary>
/// Contains all category scores and configured weights
/// required to calculate the final weighted match score.
///
/// The weights will later be mapped from the active
/// MatchingRule stored in the database.
/// </summary>
public class MatchScoreCalculationInput
{
    // ---------------------------------------------------------
    // CATEGORY RESULTS
    // ---------------------------------------------------------

    public CategoryScoreResult Skills { get; set; } = new();

    public CategoryScoreResult Experience { get; set; } = new();

    public CategoryScoreResult Education { get; set; } = new();

    public CategoryScoreResult Certification { get; set; } = new();

    public CategoryScoreResult Location { get; set; } = new();

    public CategoryScoreResult Projects { get; set; } = new();

    public CategoryScoreResult ProfileCompletion { get; set; } = new();


    // ---------------------------------------------------------
    // CATEGORY WEIGHTS
    // ---------------------------------------------------------

    public decimal SkillsWeight { get; set; }

    public decimal ExperienceWeight { get; set; }

    public decimal EducationWeight { get; set; }

    public decimal CertificationWeight { get; set; }

    public decimal LocationWeight { get; set; }

    public decimal ProjectsWeight { get; set; }

    public decimal ProfileCompletionWeight { get; set; }


    /// <summary>
    /// Total of all configured matching weights.
    /// A valid configuration must equal 100.
    /// </summary>
    public decimal TotalWeight =>
        SkillsWeight +
        ExperienceWeight +
        EducationWeight +
        CertificationWeight +
        LocationWeight +
        ProjectsWeight +
        ProfileCompletionWeight;


    /// <summary>
    /// Checks whether all category weights together
    /// form a complete 100 percent scoring model.
    /// </summary>
    public bool HasValidTotalWeight()
    {
        return TotalWeight == 100m;
    }
}
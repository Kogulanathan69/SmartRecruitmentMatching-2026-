namespace NexHire.Application.Matching;

/// <summary>
/// Contains only the project information required
/// by the project matching calculator.
///
/// This model is intentionally independent from the
/// team's final Project database entities.
///
/// When the actual project model is available,
/// only the mapping layer should need to change.
/// </summary>
public class ProjectMatchInput
{
    /// <summary>
    /// Number of candidate projects that are relevant
    /// to the job requirements.
    /// </summary>
    public int RelevantProjectCount { get; set; }

    /// <summary>
    /// Target number of relevant projects expected
    /// for a full Projects category score.
    ///
    /// The final value will come from the matching rule
    /// or team business requirement during integration.
    /// </summary>
    public int TargetProjectCount { get; set; }
}
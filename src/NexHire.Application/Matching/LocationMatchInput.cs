namespace NexHire.Application.Matching;

/// <summary>
/// Contains only the location information required
/// by the location matching calculator.
///
/// This keeps the matching logic independent from
/// the team's database location entities.
/// </summary>
public class LocationMatchInput
{
    /// <summary>
    /// Candidate's preferred or current location.
    /// </summary>
    public string CandidateLocation { get; set; } = string.Empty;

    /// <summary>
    /// Location required or preferred by the job.
    /// </summary>
    public string JobLocation { get; set; } = string.Empty;
}
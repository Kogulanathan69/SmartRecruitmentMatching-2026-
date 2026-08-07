namespace NexHire.Domain.Entities;

public class MatchScoreDetail
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MatchResultId { get; set; }

    public string Category { get; set; } = string.Empty;

    public decimal RawScore { get; set; }

    public decimal Weight { get; set; }

    public decimal WeightedPoints { get; set; }

    public decimal MaximumWeightedPoints { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;
}
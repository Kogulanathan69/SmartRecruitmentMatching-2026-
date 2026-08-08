namespace NexHire.Application.DTOs.Matching;

public class CompareCandidatesRequestDto
{
    public Guid JobId { get; set; }

    public List<Guid> ApplicationIds { get; set; } = new();
}
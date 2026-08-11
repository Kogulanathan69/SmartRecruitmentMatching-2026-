namespace NexHire.Application.DTOs.Matching;

public sealed class CompareCandidatesRequestDto
{
    public List<Guid> ApplicationIds { get; set; } = new();
}

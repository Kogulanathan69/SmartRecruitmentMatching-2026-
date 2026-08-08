namespace NexHire.Application.DTOs.Resume;

public class ResumeCompletenessDto
{
    public int Score { get; set; }
    public string Rating { get; set; } = string.Empty;
    public List<string> CompletedSections { get; set; } = new();
    public List<string> MissingSections { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

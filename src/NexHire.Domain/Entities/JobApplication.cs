namespace NexHire.Domain.Entities;

public class JobApplication
{
    public Guid JobApplicationId { get; set; } = Guid.NewGuid();
    public Guid CandidateId { get; set; }
    public JobSeekerProfile Candidate { get; set; } = null!;
    public Guid VacancyId { get; set; }
    public Job Vacancy { get; set; } = null!;
    public string Status { get; set; } = "Submitted";
    public string IdempotencyKey { get; set; } = string.Empty;
    public decimal MatchScoreSnapshot { get; set; }
    public string MatchRuleVersion { get; set; } = "RM-1.0";
    public DateTime AppliedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public ICollection<ApplicationStatusHistory> StatusHistory { get; set; } = new List<ApplicationStatusHistory>();
    public ContactRequest? ContactRequest { get; set; }
}

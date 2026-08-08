namespace NexHire.Domain.Entities;

public class JobApplication
{
    public Guid JobApplicationId { get; set; }

    public Guid CandidateId { get; set; }

    public Guid VacancyId { get; set; }

    public string Status { get; set; } = "Submitted";

    public DateTime AppliedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<ApplicationStatusHistory> StatusHistory { get; set; }
        = new List<ApplicationStatusHistory>();

    public ContactRequest? ContactRequest { get; set; }
}
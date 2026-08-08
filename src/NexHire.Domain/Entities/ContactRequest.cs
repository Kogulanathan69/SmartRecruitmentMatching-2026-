using NexHire.Domain.Enums;

namespace NexHire.Domain.Entities;

public class ContactRequest
{
    public Guid Id { get; set; }
    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public Guid EmployerUserId { get; set; }
    public Guid CandidateUserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public ContactRequestStatus Status { get; set; } = ContactRequestStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? DecisionAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

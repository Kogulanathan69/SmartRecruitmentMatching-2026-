using NexHire.Domain.Enums;

namespace NexHire.Domain.Entities;

public class CompanyVerification
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    public bool RegistrationDocumentVerified { get; set; }
    public string? DeclarationName { get; set; }
    public string? DeclarationDesignation { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? Remarks { get; set; }
}

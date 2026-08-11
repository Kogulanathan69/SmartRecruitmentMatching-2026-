namespace NexHire.Application.DTOs.Company;

public class CompanyVerificationStatusDto
{
    public Guid CompanyId { get; set; }
    public string CompanyStatus { get; set; } = string.Empty;
    public string VerificationStatus { get; set; } = string.Empty;
    public int TrustScore { get; set; }
    public string TrustLevel { get; set; } = string.Empty;
    public bool RegistrationDocumentUploaded { get; set; }
    public bool RegistrationDocumentVerified { get; set; }
    public bool OfficialEmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public bool WebsiteDomainMatched { get; set; }
    public bool RegisteredAddressVerified { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? Remarks { get; set; }
    public IReadOnlyList<string> MissingRequirements { get; set; } = Array.Empty<string>();
}

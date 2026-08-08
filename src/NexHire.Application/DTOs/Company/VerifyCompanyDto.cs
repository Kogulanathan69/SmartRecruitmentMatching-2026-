namespace NexHire.Application.DTOs.Company;

public class VerifyCompanyDto
{
    /// <summary>Verified, Rejected, MoreInformationRequired, or Suspended.</summary>
    public string Status { get; set; } = string.Empty;
    public bool RegistrationDocumentVerified { get; set; }
    public bool OfficialEmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public bool WebsiteDomainMatched { get; set; }
    public bool RegisteredAddressVerified { get; set; }
    public string? Remarks { get; set; }
}

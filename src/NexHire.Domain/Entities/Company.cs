using NexHire.Domain.Enums;

namespace NexHire.Domain.Entities;

public class Company
{
    public Guid Id { get; set; }
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? OfficialEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public string? RegisteredAddress { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? CompanySize { get; set; }
    public string? LogoUrl { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool IsDomainMatched { get; set; }
    public bool IsAddressVerified { get; set; }
    public int TrustScore { get; set; }
    public string TrustLevel { get; set; } = "Review Required";

    public CompanyStatus Status { get; set; } = CompanyStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<CompanyDocument> Documents { get; set; } = new List<CompanyDocument>();
    public CompanyVerification? Verification { get; set; }
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public ICollection<TalentPoolEntry> TalentPoolEntries { get; set; } = new List<TalentPoolEntry>();
}

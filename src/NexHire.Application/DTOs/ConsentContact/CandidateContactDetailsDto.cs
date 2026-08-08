namespace NexHire.Application.DTOs.ConsentContact;

public class CandidateContactDetailsDto
{
    public Guid ContactRequestId { get; set; }
    public Guid ApplicationId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

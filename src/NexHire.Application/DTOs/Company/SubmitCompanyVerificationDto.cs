namespace NexHire.Application.DTOs.Company;

public class SubmitCompanyVerificationDto
{
    public string DeclarationName { get; set; } = string.Empty;
    public string DeclarationDesignation { get; set; } = string.Empty;
    public bool InformationIsAccurate { get; set; }
}

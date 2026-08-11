namespace NexHire.Application.DTOs.JobSeeker;

public class UpdateJobSeekerProfileDto
{
    public string? Headline { get; set; }
    public string? Summary { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public int? YearsOfExperience { get; set; }
    public decimal? ExpectedSalaryMin { get; set; }
    public decimal? ExpectedSalaryMax { get; set; }
    public bool? IsProfilePublic { get; set; }
    public bool? IsOpenToWork { get; set; }

    // PUT keeps omitted values for backward compatibility. These flags
    // allow clients to intentionally clear nullable profile fields.
    public bool ClearDateOfBirth { get; set; }
    public bool ClearExpectedSalaryMin { get; set; }
    public bool ClearExpectedSalaryMax { get; set; }
}

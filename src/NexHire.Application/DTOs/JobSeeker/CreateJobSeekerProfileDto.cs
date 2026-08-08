namespace NexHire.Application.DTOs.JobSeeker;

public class CreateJobSeekerProfileDto
{
    public string? Headline { get; set; }
    public string? Summary { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal? ExpectedSalaryMin { get; set; }
    public decimal? ExpectedSalaryMax { get; set; }
    public bool IsProfilePublic { get; set; } = true;
    public bool IsOpenToWork { get; set; } = true;
}

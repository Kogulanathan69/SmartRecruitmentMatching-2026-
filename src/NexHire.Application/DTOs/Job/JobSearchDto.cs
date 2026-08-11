namespace NexHire.Application.DTOs.Job;

public sealed class JobSearchDto
{
    public string? Query { get; set; }

    public string? Location { get; set; }

    public string? Country { get; set; }

    public string? EmploymentType { get; set; }

    public string? Skill { get; set; }

    public bool? IsRemote { get; set; }

    public int? ExperienceYears { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

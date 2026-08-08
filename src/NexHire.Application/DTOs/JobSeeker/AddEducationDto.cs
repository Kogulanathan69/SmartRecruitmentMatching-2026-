namespace NexHire.Application.DTOs.JobSeeker;

public class AddEducationDto
{
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string? FieldOfStudy { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? GradeOrGpa { get; set; }
}

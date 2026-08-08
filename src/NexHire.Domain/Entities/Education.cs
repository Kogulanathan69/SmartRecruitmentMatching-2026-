namespace NexHire.Domain.Entities;

public class Education
{
    public Guid Id { get; set; }
    public Guid JobSeekerProfileId { get; set; }
    public JobSeekerProfile JobSeekerProfile { get; set; } = null!;

    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;

    // Ordered qualification level used by the matching engine.
    // 0 means the level has not been specified.
    // Higher values represent higher qualification levels.
    public int EducationLevel { get; set; }

    public string? FieldOfStudy { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? GradeOrGpa { get; set; }
}

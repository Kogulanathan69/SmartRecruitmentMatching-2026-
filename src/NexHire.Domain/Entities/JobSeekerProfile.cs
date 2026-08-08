namespace NexHire.Domain.Entities;

public class JobSeekerProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string? Headline { get; set; }
    public string? Summary { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public decimal? ExpectedSalaryMin { get; set; }
    public decimal? ExpectedSalaryMax { get; set; }
    public int YearsOfExperience { get; set; }
    public bool IsProfilePublic { get; set; } = true;
    public bool IsOpenToWork { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Education> Educations { get; set; } = new List<Education>();
    public ICollection<Experience> Experiences { get; set; } = new List<Experience>();
    public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Certification> Certifications { get; set; } = new List<Certification>();
    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    public ICollection<TalentPoolEntry> TalentPoolEntries { get; set; } = new List<TalentPoolEntry>();
}

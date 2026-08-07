namespace NexHire.Application.Matching;

/// <summary>
/// Contains all information required to calculate
/// one complete candidate-to-job match.
///
/// This model keeps MatchingService independent
/// from unfinished team database entities.
/// Later, repository data will be mapped into this model.
/// </summary>
public class MatchingCalculationInput
{
    public Guid JobSeekerProfileId { get; set; }

    public Guid JobId { get; set; }

    public EligibilityInput Eligibility { get; set; } = new();

    public SkillMatchInput Skills { get; set; } = new();

    public ExperienceMatchInput Experience { get; set; } = new();

    public EducationMatchInput Education { get; set; } = new();

    public CertificationMatchInput Certification { get; set; } = new();

    public LocationMatchInput Location { get; set; } = new();

    public ProjectMatchInput Projects { get; set; } = new();

    public ProfileCompletionMatchInput ProfileCompletion { get; set; } = new();

    public decimal SkillsWeight { get; set; }

    public decimal ExperienceWeight { get; set; }

    public decimal EducationWeight { get; set; }

    public decimal CertificationWeight { get; set; }

    public decimal LocationWeight { get; set; }

    public decimal ProjectsWeight { get; set; }

    public decimal ProfileCompletionWeight { get; set; }

    public decimal TotalWeight =>
        SkillsWeight +
        ExperienceWeight +
        EducationWeight +
        CertificationWeight +
        LocationWeight +
        ProjectsWeight +
        ProfileCompletionWeight;
}
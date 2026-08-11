namespace NexHire.Application.Matching;

public class EligibilityResult
{
    public bool IsEligible { get; set; }

    public List<string> FailureReasons { get; set; } = new();

    public List<string> MatchedMandatorySkills { get; set; } = new();

    public List<string> MissingMandatorySkills { get; set; } = new();

    public static EligibilityResult Eligible()
    {
        return new EligibilityResult
        {
            IsEligible = true
        };
    }

    public static EligibilityResult NotEligible(params string[] reasons)
    {
        return new EligibilityResult
        {
            IsEligible = false,
            FailureReasons = reasons.ToList()
        };
    }
}
namespace NexHire.Application.Common;

public static class MatchingConstants
{
    // All matching scores are represented from 0 to 100.
    public const decimal MinimumScore = 0m;
    public const decimal MaximumScore = 100m;

    // Skill score rule defined by the matching design.
    public const decimal MandatorySkillShare = 0.80m;
    public const decimal PreferredSkillShare = 0.20m;

    // Candidate recommendation bands.
    public const decimal HighlyRecommendedMinimum = 90m;
    public const decimal RecommendedForReviewMinimum = 75m;
    public const decimal ConsiderWithCautionMinimum = 60m;

    public const string HighlyRecommended = "Highly Recommended";
    public const string RecommendedForReview = "Recommended for Review";
    public const string ConsiderWithCaution = "Consider with Caution";
    public const string NotRecommended = "Not Recommended";
    public const string NotEligible = "Not Eligible";

    // Candidate comparison rules.
    public const int MinimumCandidatesToCompare = 2;
    public const int MaximumCandidatesToCompare = 4;

    // Matching score categories.
    public const string Skills = "Skills";
    public const string Experience = "Experience";
    public const string Education = "Education";
    public const string Certification = "Certification";
    public const string Location = "Location";
    public const string Projects = "Projects";
    public const string ProfileCompletion = "Profile Completion";
}
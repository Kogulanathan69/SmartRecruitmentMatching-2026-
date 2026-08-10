namespace NexHire.Application.Common;

public sealed class Member5RulesOptions
{
    public int MinimumInterviewScore { get; init; } = 0;
    public int MaximumInterviewScore { get; init; } = 100;
    public int MaximumPageSize { get; init; } = 100;

    public string[] AllowedInterviewApplicationStatuses { get; init; } = ["Shortlisted"];
    public string[] AllowedOfferApplicationStatuses { get; init; } =
        ["InterviewCompleted", "Selected", "Shortlisted"];

    public string[] CompanyRoles { get; init; } = ["Company", "Employer"];
    public string[] CandidateRoles { get; init; } = ["JobSeeker", "Candidate"];
}

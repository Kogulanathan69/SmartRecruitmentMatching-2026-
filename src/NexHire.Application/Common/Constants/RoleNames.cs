namespace NexHire.Application.Common;

public static class RoleNames
{
    public const string Admin = "Admin";

    public const string Employer = "Employer";

    public const string JobSeeker = "JobSeeker";

    // Backward-compatible alias used by older code.
    public const string Candidate = JobSeeker;

    public const string Company = Employer;
}

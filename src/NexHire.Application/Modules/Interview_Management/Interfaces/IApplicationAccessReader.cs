namespace NexHire.Application.Interfaces.Services;

public sealed record ApplicationAccessSnapshot(
    Guid ApplicationId,
    Guid CompanyId,
    Guid CandidateProfileId,
    string Status,
    string? JobTitle,
    string? CompanyName,
    string? CandidateName);

public interface IApplicationAccessReader
{
    Task<ApplicationAccessSnapshot?> GetAsync(
        Guid applicationId,
        CancellationToken cancellationToken = default);
}

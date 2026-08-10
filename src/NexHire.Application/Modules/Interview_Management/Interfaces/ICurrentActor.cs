namespace NexHire.Application.Interfaces.Services;

public interface ICurrentActor
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    string Role { get; }
    Guid? CompanyId { get; }
    Guid? CandidateProfileId { get; }
}

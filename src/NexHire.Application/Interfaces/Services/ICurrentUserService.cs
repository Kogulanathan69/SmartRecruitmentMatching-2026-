namespace NexHire.Application.Interfaces.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string role);
}
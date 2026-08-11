namespace NexHire.Application.Interfaces.Services;

public interface IResumeFileStorage
{
    Task<string> SaveAsync(
        Guid userId,
        string extension,
        byte[] content,
        CancellationToken cancellationToken = default);

    Task<byte[]> ReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default);
}

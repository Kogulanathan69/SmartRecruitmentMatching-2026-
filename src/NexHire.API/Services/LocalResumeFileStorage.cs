using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Services;

public sealed class LocalResumeFileStorage : IResumeFileStorage
{
    private readonly string _rootPath;

    public LocalResumeFileStorage(IWebHostEnvironment environment)
    {
        _rootPath = Path.Combine(environment.ContentRootPath, "App_Data", "resumes");
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(
        Guid userId,
        string extension,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        extension = extension.ToLowerInvariant();

        if (extension is not ".pdf" and not ".doc" and not ".docx")
            throw new InvalidOperationException("Unsupported CV extension.");

        var userFolder = userId.ToString("N");
        var directory = Path.Combine(_rootPath, userFolder);
        Directory.CreateDirectory(directory);

        var storedName = $"{Guid.NewGuid():N}{extension}";
        var storageKey = $"{userFolder}/{storedName}";
        var fullPath = ResolveSafePath(storageKey);

        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);
        return storageKey;
    }

    public async Task<byte[]> ReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveSafePath(storageKey);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Stored CV file was not found.");

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveSafePath(storageKey);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    private string ResolveSafePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || Path.IsPathRooted(storageKey))
            throw new InvalidOperationException("Invalid CV storage path.");

        var normalized = storageKey
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        var root = Path.GetFullPath(_rootPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, normalized));

        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid CV storage path.");

        return fullPath;
    }
}

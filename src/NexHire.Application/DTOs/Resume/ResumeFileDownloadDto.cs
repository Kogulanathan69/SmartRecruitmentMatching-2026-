namespace NexHire.Application.DTOs.Resume;

public sealed class ResumeFileDownloadDto
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
    public string DownloadName { get; set; } = "resume";
}

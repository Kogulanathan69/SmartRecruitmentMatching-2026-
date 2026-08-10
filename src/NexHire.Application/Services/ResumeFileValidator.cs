using System.IO.Compression;
using NexHire.Application.Common.Exceptions;

namespace NexHire.Application.Services;

public static class ResumeFileValidator
{
    public const int MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly byte[] PdfHeader =
    {
        0x25, 0x50, 0x44, 0x46, 0x2D
    };

    private static readonly byte[] DocHeader =
    {
        0xD0, 0xCF, 0x11, 0xE0,
        0xA1, 0xB1, 0x1A, 0xE1
    };

    public static string ValidateAndGetExtension(
        string originalFileName,
        byte[] content)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ValidationException("CV file name is required.");

        if (content is null || content.Length == 0)
            throw new ValidationException("CV file is empty.");

        if (content.Length > MaxFileSizeBytes)
            throw new ValidationException("CV file cannot exceed 5 MB.");

        var safeName = Path.GetFileName(originalFileName);
        var extension = Path.GetExtension(safeName).ToLowerInvariant();

        if (extension is not ".pdf" and not ".doc" and not ".docx")
            throw new ValidationException("Only PDF, DOC and DOCX CV files are allowed.");

        var valid = extension switch
        {
            ".pdf" => HasPrefix(content, PdfHeader),
            ".doc" => HasPrefix(content, DocHeader),
            ".docx" => IsValidDocx(content),
            _ => false
        };

        if (!valid)
            throw new ValidationException("The uploaded file content does not match its file type.");

        return extension;
    }

    public static string GetContentType(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };

    private static bool HasPrefix(byte[] content, byte[] expected)
    {
        if (content.Length < expected.Length)
            return false;

        for (var i = 0; i < expected.Length; i++)
        {
            if (content[i] != expected[i])
                return false;
        }

        return true;
    }

    private static bool IsValidDocx(byte[] content)
    {
        if (content.Length < 4 || content[0] != 0x50 || content[1] != 0x4B)
            return false;

        try
        {
            using var stream = new MemoryStream(content);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            var hasContentTypes = archive.Entries.Any(e =>
                e.FullName.Equals("[Content_Types].xml", StringComparison.OrdinalIgnoreCase));

            var hasDocument = archive.Entries.Any(e =>
                e.FullName.Equals("word/document.xml", StringComparison.OrdinalIgnoreCase));

            return hasContentTypes && hasDocument;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }
}

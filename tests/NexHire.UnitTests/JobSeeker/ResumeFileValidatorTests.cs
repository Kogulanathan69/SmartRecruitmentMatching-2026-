using System.IO.Compression;
using System.Text;
using FluentAssertions;
using NexHire.Application.Common.Exceptions;
using NexHire.Application.Services;
using Xunit;

namespace NexHire.UnitTests.JobSeeker;

public class ResumeFileValidatorTests
{
    [Fact]
    public void ValidPdf_IsAccepted()
    {
        var bytes = Encoding.ASCII.GetBytes("%PDF-1.7 test");
        ResumeFileValidator.ValidateAndGetExtension("candidate.pdf", bytes).Should().Be(".pdf");
    }

    [Fact]
    public void FakePdf_IsRejected()
    {
        var bytes = Encoding.ASCII.GetBytes("not pdf");
        var action = () => ResumeFileValidator.ValidateAndGetExtension("candidate.pdf", bytes);
        action.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidLegacyDoc_IsAccepted()
    {
        var bytes = new byte[]
        {
            0xD0, 0xCF, 0x11, 0xE0,
            0xA1, 0xB1, 0x1A, 0xE1,
            0x00
        };

        ResumeFileValidator.ValidateAndGetExtension("candidate.doc", bytes).Should().Be(".doc");
    }

    [Fact]
    public void ValidDocx_IsAccepted()
    {
        ResumeFileValidator.ValidateAndGetExtension("candidate.docx", CreateDocx()).Should().Be(".docx");
    }

    [Fact]
    public void FakeDocx_IsRejected()
    {
        var bytes = Encoding.ASCII.GetBytes("PK fake");
        var action = () => ResumeFileValidator.ValidateAndGetExtension("candidate.docx", bytes);
        action.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void UnsupportedExtension_IsRejected()
    {
        var bytes = Encoding.ASCII.GetBytes("sample");
        var action = () => ResumeFileValidator.ValidateAndGetExtension("candidate.exe", bytes);
        action.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void FileAboveFiveMb_IsRejected()
    {
        var bytes = new byte[ResumeFileValidator.MaxFileSizeBytes + 1];
        var action = () => ResumeFileValidator.ValidateAndGetExtension("candidate.pdf", bytes);
        action.Should().Throw<BusinessRuleException>();
    }

    private static byte[] CreateDocx()
    {
        using var memory = new MemoryStream();

        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            using (var writer = new StreamWriter(archive.CreateEntry("[Content_Types].xml").Open()))
                writer.Write("<Types />");

            using (var writer = new StreamWriter(archive.CreateEntry("word/document.xml").Open()))
                writer.Write("<document />");
        }

        return memory.ToArray();
    }
}

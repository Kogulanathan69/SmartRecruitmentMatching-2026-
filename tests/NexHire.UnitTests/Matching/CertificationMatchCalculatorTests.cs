using NexHire.Application.Matching;
using NexHire.Infrastructure.Matching;
using Xunit;

namespace NexHire.UnitTests.Matching;

/// <summary>
/// Tests the deterministic Certification category calculation.
/// </summary>
public class CertificationMatchCalculatorTests
{
    private readonly CertificationMatchCalculator _calculator = new();

    [Fact]
    public void Calculate_WhenNoCertificationIsRequired_ReturnsFullScore()
    {
        var input = new CertificationMatchInput
        {
            RequiredCertifications = new List<string>(),
            CandidateCertifications = new List<string>()
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateMatchesAllRequiredCertifications_ReturnsFullScore()
    {
        var input = new CertificationMatchInput
        {
            RequiredCertifications = new List<string>
            {
                "AZ-900",
                "AWS Cloud Practitioner"
            },
            CandidateCertifications = new List<string>
            {
                "AZ-900",
                "AWS Cloud Practitioner"
            }
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateMatchesThreeOfFour_ReturnsSeventyFive()
    {
        var input = new CertificationMatchInput
        {
            RequiredCertifications = new List<string>
            {
                "Certification A",
                "Certification B",
                "Certification C",
                "Certification D"
            },
            CandidateCertifications = new List<string>
            {
                "Certification A",
                "Certification B",
                "Certification C"
            }
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(75m, result.RawScore);
        Assert.Equal("Good", result.Status);
    }

    [Fact]
    public void Calculate_WhenCandidateMatchesOneOfTwo_ReturnsFifty()
    {
        var input = new CertificationMatchInput
        {
            RequiredCertifications = new List<string>
            {
                "Certification A",
                "Certification B"
            },
            CandidateCertifications = new List<string>
            {
                "Certification A"
            }
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(50m, result.RawScore);
        Assert.Equal("Needs Improvement", result.Status);
    }

    [Fact]
    public void Calculate_WhenCertificationNamesUseDifferentCase_StillMatches()
    {
        var input = new CertificationMatchInput
        {
            RequiredCertifications = new List<string>
            {
                "AZ-900"
            },
            CandidateCertifications = new List<string>
            {
                "az-900"
            }
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenRequiredCertificationsContainDuplicates_IgnoresDuplicates()
    {
        var input = new CertificationMatchInput
        {
            RequiredCertifications = new List<string>
            {
                "AZ-900",
                "AZ-900",
                " AWS Cloud Practitioner "
            },
            CandidateCertifications = new List<string>
            {
                "AZ-900",
                "AWS Cloud Practitioner"
            }
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }
}
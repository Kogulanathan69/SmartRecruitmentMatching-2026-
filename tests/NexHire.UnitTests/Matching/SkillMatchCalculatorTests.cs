using NexHire.Application.Matching;
using NexHire.Infrastructure.Matching;
using Xunit;

namespace NexHire.UnitTests.Matching;

/// <summary>
/// Tests the Skills category calculation.
///
/// These tests protect the 80/20 mandatory/preferred scoring rule
/// and make future team integration safer.
/// </summary>
public class SkillMatchCalculatorTests
{
    private readonly SkillMatchCalculator _calculator = new();

    [Fact]
    public void Calculate_WhenAllSkillsMatch_ReturnsFullScore()
    {
        var input = new SkillMatchInput
        {
            MandatorySkills =
            {
                new RequiredSkillInput
                {
                    SkillName = "C#",
                    MinimumProficiencyLevel = 3
                }
            },

            PreferredSkills =
            {
                new RequiredSkillInput
                {
                    SkillName = "SQL",
                    MinimumProficiencyLevel = 2
                }
            },

            CandidateSkills =
            {
                new CandidateSkillInput
                {
                    SkillName = "C#",
                    ProficiencyLevel = 4
                },

                new CandidateSkillInput
                {
                    SkillName = "SQL",
                    ProficiencyLevel = 3
                }
            }
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenMandatoryHalfAndPreferredFull_UsesEightyTwentyRule()
    {
        var input = new SkillMatchInput
        {
            MandatorySkills =
            {
                new RequiredSkillInput
                {
                    SkillName = "C#",
                    MinimumProficiencyLevel = 3
                },

                new RequiredSkillInput
                {
                    SkillName = "ASP.NET Core",
                    MinimumProficiencyLevel = 3
                }
            },

            PreferredSkills =
            {
                new RequiredSkillInput
                {
                    SkillName = "SQL",
                    MinimumProficiencyLevel = 2
                }
            },

            CandidateSkills =
            {
                new CandidateSkillInput
                {
                    SkillName = "C#",
                    ProficiencyLevel = 4
                },

                new CandidateSkillInput
                {
                    SkillName = "SQL",
                    ProficiencyLevel = 3
                }
            }
        };

        var result = _calculator.Calculate(input);

        // Mandatory = 50
        // Preferred = 100
        // (50 × 80%) + (100 × 20%) = 60
        Assert.Equal(60m, result.RawScore);
        Assert.Equal("Partial", result.Status);
    }

    [Fact]
    public void Calculate_WhenOnlyMandatorySkillsExist_UsesMandatoryScoreOnly()
    {
        var input = new SkillMatchInput
        {
            MandatorySkills =
            {
                new RequiredSkillInput
                {
                    SkillName = "C#",
                    MinimumProficiencyLevel = 3
                },

                new RequiredSkillInput
                {
                    SkillName = "SQL",
                    MinimumProficiencyLevel = 2
                }
            },

            CandidateSkills =
            {
                new CandidateSkillInput
                {
                    SkillName = "C#",
                    ProficiencyLevel = 4
                }
            }
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(50m, result.RawScore);
        Assert.Equal("Needs Improvement", result.Status);
    }

    [Fact]
    public void Calculate_WhenOnlyPreferredSkillsExist_UsesPreferredScoreOnly()
    {
        var input = new SkillMatchInput
        {
            PreferredSkills =
            {
                new RequiredSkillInput
                {
                    SkillName = "SQL",
                    MinimumProficiencyLevel = 2
                }
            },

            CandidateSkills =
            {
                new CandidateSkillInput
                {
                    SkillName = "SQL",
                    ProficiencyLevel = 3
                }
            }
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }

    [Fact]
    public void Calculate_WhenProficiencyIsTooLow_DoesNotCountSkillAsMatched()
    {
        var input = new SkillMatchInput
        {
            MandatorySkills =
            {
                new RequiredSkillInput
                {
                    SkillName = "C#",
                    MinimumProficiencyLevel = 4
                }
            },

            CandidateSkills =
            {
                new CandidateSkillInput
                {
                    SkillName = "C#",
                    ProficiencyLevel = 2
                }
            }
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(0m, result.RawScore);
        Assert.Equal("Needs Improvement", result.Status);
    }

    [Fact]
    public void Calculate_WhenNoSkillRequirementsExist_ReturnsFullScore()
    {
        var input = new SkillMatchInput();

        var result = _calculator.Calculate(input);

        Assert.Equal(100m, result.RawScore);
        Assert.Equal("Strong", result.Status);
    }
}
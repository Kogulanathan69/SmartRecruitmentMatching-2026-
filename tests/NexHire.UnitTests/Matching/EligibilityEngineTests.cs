using NexHire.Application.Matching;
using Xunit;

namespace NexHire.UnitTests.Matching;

/// <summary>
/// Tests the mandatory eligibility rules.
///
/// These tests protect the matching logic from accidental changes
/// when other team members update their Job, Skill, Experience
/// or Education implementations.
/// </summary>
public class EligibilityEngineTests
{
    private readonly EligibilityEngine _engine = new();

    [Fact]
    public void Evaluate_WhenAllRequirementsAreMet_ReturnsEligible()
    {
        var input = CreateValidInput();

        var result = _engine.Evaluate(input);

        Assert.True(result.IsEligible);
        Assert.Empty(result.FailureReasons);
        Assert.Contains("C#", result.MatchedMandatorySkills);
        Assert.Contains("ASP.NET Core", result.MatchedMandatorySkills);
        Assert.Empty(result.MissingMandatorySkills);
    }

    [Fact]
    public void Evaluate_WhenJobIsUnavailable_ReturnsNotEligible()
    {
        var input = CreateValidInput();

        input.IsJobAvailable = false;
        input.JobUnavailableReason = "The job has expired.";

        var result = _engine.Evaluate(input);

        Assert.False(result.IsEligible);
        Assert.Contains("The job has expired.", result.FailureReasons);
    }

    [Fact]
    public void Evaluate_WhenMandatorySkillIsMissing_ReturnsNotEligible()
    {
        var input = CreateValidInput();

        input.CandidateSkills.RemoveAll(
            skill => skill.SkillName == "ASP.NET Core");

        var result = _engine.Evaluate(input);

        Assert.False(result.IsEligible);
        Assert.Contains("ASP.NET Core", result.MissingMandatorySkills);
        Assert.Contains(
            result.FailureReasons,
            reason => reason.Contains("ASP.NET Core"));
    }

    [Fact]
    public void Evaluate_WhenSkillProficiencyIsTooLow_ReturnsNotEligible()
    {
        var input = CreateValidInput();

        var skill = input.CandidateSkills
            .First(x => x.SkillName == "C#");

        skill.ProficiencyLevel = 1;

        var result = _engine.Evaluate(input);

        Assert.False(result.IsEligible);
        Assert.Contains("C#", result.MissingMandatorySkills);
    }

    [Fact]
    public void Evaluate_WhenExperienceIsBelowMinimum_ReturnsNotEligible()
    {
        var input = CreateValidInput();

        input.CandidateExperienceYears = 1;
        input.MinimumExperienceYears = 3;

        var result = _engine.Evaluate(input);

        Assert.False(result.IsEligible);
        Assert.Contains(
            result.FailureReasons,
            reason => reason.Contains("Minimum experience"));
    }

    [Fact]
    public void Evaluate_WhenEducationIsBelowMinimum_ReturnsNotEligible()
    {
        var input = CreateValidInput();

        input.CandidateEducationLevel = 1;
        input.MinimumEducationLevel = 3;

        var result = _engine.Evaluate(input);

        Assert.False(result.IsEligible);
        Assert.Contains(
            "Minimum education requirement is not met.",
            result.FailureReasons);
    }

    /// <summary>
    /// Creates one valid candidate/job example.
    /// Each test changes only the value it wants to verify.
    /// This keeps the tests simple and easy to maintain.
    /// </summary>
    private static EligibilityInput CreateValidInput()
    {
        return new EligibilityInput
        {
            IsJobAvailable = true,

            CandidateExperienceYears = 4,
            MinimumExperienceYears = 2,

            CandidateEducationLevel = 3,
            MinimumEducationLevel = 2,

            MandatorySkills = new List<RequiredSkillInput>
            {
                new()
                {
                    SkillName = "C#",
                    MinimumProficiencyLevel = 3
                },
                new()
                {
                    SkillName = "ASP.NET Core",
                    MinimumProficiencyLevel = 2
                }
            },

            CandidateSkills = new List<CandidateSkillInput>
            {
                new()
                {
                    SkillName = "C#",
                    ProficiencyLevel = 4
                },
                new()
                {
                    SkillName = "ASP.NET Core",
                    ProficiencyLevel = 3
                }
            }
        };
    }
}
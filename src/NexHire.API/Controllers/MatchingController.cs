using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.DTOs.Matching;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Mappings;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/matching")]
[Authorize(Roles = "Employer,Admin")]
public class MatchingController : ControllerBase
{
    private readonly IMatchingService _matchingService;

    public MatchingController(
        IMatchingService matchingService)
    {
        _matchingService =
            matchingService ??
            throw new ArgumentNullException(
                nameof(matchingService));
    }

    /// <summary>
    /// Preview a candidate-to-job match without storing it.
    /// </summary>
    [HttpGet(
        "jobs/{jobId:guid}/candidates/{jobSeekerProfileId:guid}")]
    public async Task<IActionResult> CalculateMatch(
        Guid jobId,
        Guid jobSeekerProfileId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _matchingService.CalculateMatchAsync(
                    jobId,
                    jobSeekerProfileId,
                    cancellationToken);

            if (result is null)
            {
                return NotFound(
                    new
                    {
                        message =
                            "Job or job seeker profile was not found."
                    });
            }

            return Ok(
                MatchingDtoMapper
                    .ToMatchScoreResponseDto(result));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(
                new { message = exception.Message });
        }
    }

    /// <summary>
    /// Calculates and stores a historical matching result.
    /// </summary>
    [HttpPost(
        "jobs/{jobId:guid}/candidates/{jobSeekerProfileId:guid}/calculate")]
    public async Task<IActionResult> CalculateAndSaveMatch(
        Guid jobId,
        Guid jobSeekerProfileId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _matchingService
                    .CalculateAndSaveMatchAsync(
                        jobId,
                        jobSeekerProfileId,
                        cancellationToken);

            if (result is null)
            {
                return NotFound(
                    new
                    {
                        message =
                            "Job or job seeker profile was not found."
                    });
            }

            return Ok(
                MatchingDtoMapper
                    .ToMatchScoreResponseDto(result));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(
                new { message = exception.Message });
        }
    }

    /// <summary>
    /// Returns the highest-ranked eligible candidates for a job.
    /// Uses competition ranking: 1, 2, 2, 4.
    /// Rejected and withdrawn applications are excluded.
    /// </summary>
    [HttpGet("jobs/{jobId:guid}/rankings")]
    public async Task<IActionResult> GetRankings(
        Guid jobId,
        [FromQuery] int top = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var rankings =
                await _matchingService
                    .GetRankedCandidatesForJobAsync(
                        jobId,
                        top,
                        cancellationToken);

            return Ok(rankings);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(
                new { message = exception.Message });
        }
    }

    /// <summary>
    /// Compares two to four selected candidate applications
    /// using their latest saved matching results.
    /// </summary>
    [HttpPost("jobs/{jobId:guid}/compare")]
    public async Task<IActionResult> CompareSelectedCandidates(
        Guid jobId,
        [FromBody] CompareCandidatesRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result =
                await _matchingService
                    .CompareCandidatesForJobAsync(
                        jobId,
                        request.ApplicationIds,
                        cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(
                new { message = exception.Message });
        }
    }
}

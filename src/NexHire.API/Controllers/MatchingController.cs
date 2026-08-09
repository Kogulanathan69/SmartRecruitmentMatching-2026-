using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Mappings;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/matching")]
[Authorize(Roles = "Employer,Admin")]
public class MatchingController : ControllerBase
{
    private readonly IMatchingService _matchingService;

    public MatchingController(IMatchingService matchingService)
    {
        _matchingService =
            matchingService ??
            throw new ArgumentNullException(nameof(matchingService));
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
                MatchingDtoMapper.ToMatchScoreResponseDto(result));
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
                await _matchingService.CalculateAndSaveMatchAsync(
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
                MatchingDtoMapper.ToMatchScoreResponseDto(result));
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

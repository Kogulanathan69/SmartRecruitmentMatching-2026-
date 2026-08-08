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

    public MatchingController(
        IMatchingService matchingService)
    {
        _matchingService =
            matchingService
            ?? throw new ArgumentNullException(
                nameof(matchingService));
    }

    /// <summary>
    /// Calculates the complete match between one
    /// candidate profile and one job using database data.
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

            var response =
                MatchingDtoMapper
                    .ToMatchScoreResponseDto(result);

            return Ok(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new
                {
                    message = exception.Message
                });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(
                new
                {
                    message = exception.Message
                });
        }
    }
}
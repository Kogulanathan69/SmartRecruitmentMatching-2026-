using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.DTOs.Matching;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/admin/matching-rules")]
[Authorize(Roles = "Admin")]
public sealed class MatchingRulesController : ControllerBase
{
    private readonly IMatchingService _matchingService;

    public MatchingRulesController(
        IMatchingService matchingService)
    {
        _matchingService =
            matchingService ??
            throw new ArgumentNullException(
                nameof(matchingService));
    }

    /// <summary>
    /// Returns the currently active matching rule.
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveRule(
        CancellationToken cancellationToken)
    {
        var rule =
            await _matchingService
                .GetActiveMatchingRuleAsync(
                    cancellationToken);

        return rule is null
            ? NotFound(
                new
                {
                    message =
                        "No active matching rule is configured."
                })
            : Ok(rule);
    }

    /// <summary>
    /// Replaces the active matching rule.
    /// Seven category weights must total exactly 100.
    /// </summary>
    [HttpPut("active")]
    public async Task<IActionResult> ReplaceActiveRule(
        [FromBody] UpdateMatchingRuleRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var rule =
                await _matchingService
                    .ReplaceActiveMatchingRuleAsync(
                        request,
                        cancellationToken);

            return Ok(rule);
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

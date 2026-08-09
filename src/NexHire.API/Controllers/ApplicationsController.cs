using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.Common.Exceptions;
using NexHire.Application.DTOs.Application;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize]
public sealed class ApplicationsController
    : ControllerBase
{
    private readonly IApplicationService _applications;
    private readonly ICurrentUserService _currentUser;

    public ApplicationsController(
        IApplicationService applications,
        ICurrentUserService currentUser)
    {
        _applications = applications;
        _currentUser = currentUser;
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> Apply(
        ApplyJobDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _applications.ApplyAsync(
                    _currentUser.UserId,
                    dto,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    applicationId =
                        result.ApplicationId
                },
                result);
        }
        catch (NotFoundException exception)
        {
            return NotFound(
                new { message = exception.Message });
        }
        catch (BusinessRuleException exception)
        {
            return Conflict(
                new { message = exception.Message });
        }
    }

    [HttpGet("mine")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> Mine(
        CancellationToken cancellationToken)
    {
        return Ok(
            await _applications.GetMineAsync(
                _currentUser.UserId,
                cancellationToken));
    }

    [HttpGet("jobs/{jobId:guid}")]
    [Authorize(Roles = "Employer,Admin")]
    public async Task<IActionResult> ForJob(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(
                await _applications
                    .GetForJobAsync(
                        jobId,
                        _currentUser.UserId,
                        _currentUser.IsInRole(
                            RoleNames.Admin),
                        cancellationToken));
        }
        catch (UnauthorizedException)
        {
            return Forbid();
        }
    }

    [HttpGet("{applicationId:guid}")]
    public async Task<IActionResult> GetById(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(
                await _applications.GetByIdAsync(
                    applicationId,
                    _currentUser.UserId,
                    _currentUser.IsInRole(
                        RoleNames.Employer),
                    _currentUser.IsInRole(
                        RoleNames.Admin),
                    cancellationToken));
        }
        catch (NotFoundException exception)
        {
            return NotFound(
                new { message = exception.Message });
        }
        catch (UnauthorizedException)
        {
            return Forbid();
        }
    }
}

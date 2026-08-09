using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.Common.Exceptions;
using NexHire.Application.DTOs.Job;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize]
public sealed class JobsController : ControllerBase
{
    private readonly IJobService _jobs;
    private readonly ICurrentUserService _currentUser;

    public JobsController(
        IJobService jobs,
        ICurrentUserService currentUser)
    {
        _jobs = jobs;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] JobSearchDto search,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _jobs.SearchAsync(
                search,
                cancellationToken));
    }

    [HttpGet("mine")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> Mine(
        CancellationToken cancellationToken)
    {
        return Ok(
            await _jobs.GetMineAsync(
                _currentUser.UserId,
                cancellationToken));
    }

    [HttpGet("{jobId:guid}")]
    public async Task<IActionResult> Get(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _jobs.GetByIdAsync(
                    jobId,
                    _currentUser.UserId,
                    _currentUser.IsInRole(
                        RoleNames.Employer),
                    _currentUser.IsInRole(
                        RoleNames.Admin),
                    cancellationToken);

            return Ok(result);
        }
        catch (NotFoundException exception)
        {
            return NotFound(
                new { message = exception.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> Create(
        CreateJobDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _jobs.CreateAsync(
                    _currentUser.UserId,
                    dto,
                    cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new { jobId = result.Id },
                result);
        }
        catch (BusinessRuleException exception)
        {
            return BadRequest(
                new { message = exception.Message });
        }
        catch (UnauthorizedException)
        {
            return Forbid();
        }
    }

    [HttpPut("{jobId:guid}")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> Update(
        Guid jobId,
        UpdateJobDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(
                await _jobs.UpdateAsync(
                    jobId,
                    _currentUser.UserId,
                    dto,
                    cancellationToken));
        }
        catch (BusinessRuleException exception)
        {
            return BadRequest(
                new { message = exception.Message });
        }
        catch (UnauthorizedException)
        {
            return Forbid();
        }
    }

    [HttpPatch("{jobId:guid}/close")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> Close(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(
                await _jobs.CloseAsync(
                    jobId,
                    _currentUser.UserId,
                    cancellationToken));
        }
        catch (UnauthorizedException)
        {
            return Forbid();
        }
    }
}

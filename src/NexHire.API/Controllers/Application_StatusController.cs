using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.DTOs.ApplicationStatus;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class Application_StatusController : ControllerBase
{
    private readonly IApplicationStatusService _service;
    private readonly ICurrentUserService _currentUser;

    private readonly IValidator<UpdateApplicationStatusRequestDto>
        _statusValidator;

    private readonly IValidator<WithdrawApplicationRequestDto>
        _withdrawValidator;

    public Application_StatusController(
        IApplicationStatusService service,
        ICurrentUserService currentUser,
        IValidator<UpdateApplicationStatusRequestDto> statusValidator,
        IValidator<WithdrawApplicationRequestDto> withdrawValidator)
    {
        _service = service;
        _currentUser = currentUser;
        _statusValidator = statusValidator;
        _withdrawValidator = withdrawValidator;
    }

    private Guid CurrentUserId => _currentUser.UserId;

    [HttpPost("employer/applications/{applicationId:guid}/status")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> EmployerUpdateStatus(
        Guid applicationId,
        UpdateApplicationStatusRequestDto dto)
    {
        var validation =
            await _statusValidator.ValidateAsync(dto);

        if (!validation.IsValid)
        {
            return BadRequest(
                validation.Errors
                    .Select(error => error.ErrorMessage));
        }

        try
        {
            return Ok(
                await _service.UpdateByEmployerAsync(
                    applicationId,
                    CurrentUserId,
                    dto));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(
                new { message = exception.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
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

    [HttpPost("candidate/applications/{applicationId:guid}/withdraw")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> CandidateWithdraw(
        Guid applicationId,
        WithdrawApplicationRequestDto dto)
    {
        var validation =
            await _withdrawValidator.ValidateAsync(dto);

        if (!validation.IsValid)
        {
            return BadRequest(
                validation.Errors
                    .Select(error => error.ErrorMessage));
        }

        try
        {
            return Ok(
                await _service.WithdrawByCandidateAsync(
                    applicationId,
                    CurrentUserId,
                    dto));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(
                new { message = exception.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(
                new { message = exception.Message });
        }
    }

    [HttpGet("candidate/applications/{applicationId:guid}/history")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> CandidateHistory(
        Guid applicationId)
    {
        try
        {
            return Ok(
                await _service.GetCandidateHistoryAsync(
                    applicationId,
                    CurrentUserId));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(
                new { message = exception.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("employer/applications/{applicationId:guid}/history")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> EmployerHistory(
        Guid applicationId)
    {
        try
        {
            return Ok(
                await _service.GetEmployerHistoryAsync(
                    applicationId,
                    CurrentUserId));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(
                new { message = exception.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}

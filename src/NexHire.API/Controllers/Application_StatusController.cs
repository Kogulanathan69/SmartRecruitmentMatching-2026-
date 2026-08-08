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

    // ----------------------------------------------------
    // EMPLOYER - UPDATE APPLICATION STATUS
    // ----------------------------------------------------

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
                    .Select(x => x.ErrorMessage));
        }

        var result =
            await _service.UpdateByEmployerAsync(
                applicationId,
                CurrentUserId,
                dto);

        return Ok(result);
    }

    // ----------------------------------------------------
    // CANDIDATE - WITHDRAW APPLICATION
    // ----------------------------------------------------

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
                    .Select(x => x.ErrorMessage));
        }

        var result =
            await _service.WithdrawByCandidateAsync(
                applicationId,
                CurrentUserId,
                dto);

        return Ok(result);
    }

    // ----------------------------------------------------
    // CANDIDATE - APPLICATION STATUS HISTORY
    // ----------------------------------------------------

    [HttpGet("candidate/applications/{applicationId:guid}/history")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> CandidateHistory(
        Guid applicationId)
    {
        var result =
            await _service.GetCandidateHistoryAsync(
                applicationId,
                CurrentUserId);

        return Ok(result);
    }

    // ----------------------------------------------------
    // EMPLOYER - APPLICATION STATUS HISTORY
    // ----------------------------------------------------

    [HttpGet("employer/applications/{applicationId:guid}/history")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> EmployerHistory(
        Guid applicationId)
    {
        var result =
            await _service.GetEmployerHistoryAsync(
                applicationId,
                CurrentUserId);

        return Ok(result);
    }
}
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.DTOs.ConsentContact;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class Consent_ContactController : ControllerBase
{
    private readonly IConsentContactService _service;
    private readonly ICurrentUserService _currentUser;

    private readonly IValidator<CreateContactRequestDto>
        _createValidator;

    private readonly IValidator<ContactDecisionRequestDto>
        _decisionValidator;

    public Consent_ContactController(
        IConsentContactService service,
        ICurrentUserService currentUser,
        IValidator<CreateContactRequestDto> createValidator,
        IValidator<ContactDecisionRequestDto> decisionValidator)
    {
        _service = service;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _decisionValidator = decisionValidator;
    }

    private Guid CurrentUserId => _currentUser.UserId;

    [HttpPost("employer/applications/{applicationId:guid}/contact-request")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> CreateRequest(
        Guid applicationId,
        CreateContactRequestDto dto)
    {
        var validation =
            await _createValidator.ValidateAsync(dto);

        if (!validation.IsValid)
        {
            return BadRequest(
                validation.Errors
                    .Select(error => error.ErrorMessage));
        }

        try
        {
            var result =
                await _service.CreateRequestAsync(
                    applicationId,
                    CurrentUserId,
                    dto);

            return Created(
                $"/api/v1/employer/contact-requests/{result.Id}",
                result);
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

    [HttpGet("employer/contact-requests")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> EmployerRequests()
    {
        return Ok(
            await _service.GetEmployerRequestsAsync(
                CurrentUserId));
    }

    [HttpGet("employer/contact-requests/{requestId:guid}/accepted-details")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> AcceptedDetails(
        Guid requestId)
    {
        try
        {
            return Ok(
                await _service.GetAcceptedDetailsAsync(
                    requestId,
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
        catch (InvalidOperationException exception)
        {
            return Conflict(
                new { message = exception.Message });
        }
    }

    [HttpGet("candidate/contact-requests")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> CandidateRequests()
    {
        return Ok(
            await _service.GetCandidateRequestsAsync(
                CurrentUserId));
    }

    [HttpPost("candidate/contact-requests/{requestId:guid}/decision")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> Decide(
        Guid requestId,
        ContactDecisionRequestDto dto)
    {
        var validation =
            await _decisionValidator.ValidateAsync(dto);

        if (!validation.IsValid)
        {
            return BadRequest(
                validation.Errors
                    .Select(error => error.ErrorMessage));
        }

        try
        {
            return Ok(
                await _service.DecideAsync(
                    requestId,
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
}

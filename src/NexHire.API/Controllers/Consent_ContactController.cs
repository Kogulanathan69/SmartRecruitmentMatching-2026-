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

    private readonly IValidator<CreateContactRequestDto> _createValidator;
    private readonly IValidator<ContactDecisionRequestDto> _decisionValidator;

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

    // Employer - create contact request
    [HttpPost("employer/applications/{applicationId:guid}/contact-request")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> CreateRequest(
        Guid applicationId,
        CreateContactRequestDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);

        if (!validation.IsValid)
        {
            return BadRequest(
                validation.Errors.Select(x => x.ErrorMessage));
        }

        var result = await _service.CreateRequestAsync(
            applicationId,
            CurrentUserId,
            dto);

        return Created(
            $"/api/v1/employer/contact-requests/{result.Id}",
            result);
    }

    // Employer - get all contact requests
    [HttpGet("employer/contact-requests")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> EmployerRequests()
    {
        var result = await _service.GetEmployerRequestsAsync(
            CurrentUserId);

        return Ok(result);
    }

    // Employer - get accepted candidate contact details
    [HttpGet("employer/contact-requests/{requestId:guid}/accepted-details")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> AcceptedDetails(Guid requestId)
    {
        var result = await _service.GetAcceptedDetailsAsync(
            requestId,
            CurrentUserId);

        return Ok(result);
    }

    // Candidate - get contact requests
    [HttpGet("candidate/contact-requests")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> CandidateRequests()
    {
        var result = await _service.GetCandidateRequestsAsync(
            CurrentUserId);

        return Ok(result);
    }

    // Candidate - accept / decline request
    [HttpPost("candidate/contact-requests/{requestId:guid}/decision")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> Decide(
        Guid requestId,
        ContactDecisionRequestDto dto)
    {
        var validation = await _decisionValidator.ValidateAsync(dto);

        if (!validation.IsValid)
        {
            return BadRequest(
                validation.Errors.Select(x => x.ErrorMessage));
        }

        var result = await _service.DecideAsync(
            requestId,
            CurrentUserId,
            dto);

        return Ok(result);
    }
}
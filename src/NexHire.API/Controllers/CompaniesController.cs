using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.API.Services;
using NexHire.Application.Common;
using NexHire.Application.DTOs.Company;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly CurrentUserService _currentUser;
    private readonly IValidator<CreateCompanyDto> _createValidator;

    public CompaniesController(ICompanyService companyService, CurrentUserService currentUser, IValidator<CreateCompanyDto> createValidator)
    {
        _companyService = companyService;
        _currentUser = currentUser;
        _createValidator = createValidator;
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> Create(CreateCompanyDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid) return BadRequest(validation.Errors.Select(e => e.ErrorMessage));
        var result = await _companyService.CreateCompanyAsync(_currentUser.UserId, dto);
        return CreatedAtAction(nameof(GetById), new { companyId = result.Id }, result);
    }

    [HttpGet("mine")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> GetMine() =>
        Ok(await _companyService.GetByOwnerAsync(_currentUser.UserId));

    [HttpGet("{companyId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid companyId)
    {
        var result = await _companyService.GetByIdAsync(companyId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{companyId:guid}")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> Update(Guid companyId, UpdateCompanyDto dto) =>
        Ok(await _companyService.UpdateCompanyAsync(companyId, _currentUser.UserId, dto));

    [HttpPost("{companyId:guid}/documents")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> UploadDocument(Guid companyId, UploadCompanyDocumentDto dto) =>
        Ok(await _companyService.UploadDocumentAsync(companyId, _currentUser.UserId, dto));

    [HttpPost("{companyId:guid}/verify-email")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> VerifyEmail(Guid companyId)
    {
        await _companyService.MarkEmailVerifiedAsync(companyId, _currentUser.UserId);
        return NoContent();
    }

    [HttpPost("{companyId:guid}/verify-phone")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> VerifyPhone(Guid companyId)
    {
        await _companyService.MarkPhoneVerifiedAsync(companyId, _currentUser.UserId);
        return NoContent();
    }

    [HttpPost("{companyId:guid}/submit-verification")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> SubmitVerification(Guid companyId, SubmitCompanyVerificationDto dto) =>
        Ok(await _companyService.SubmitVerificationAsync(companyId, _currentUser.UserId, dto));

    [HttpGet("{companyId:guid}/verification-status")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> VerificationStatus(Guid companyId) =>
        Ok(await _companyService.GetVerificationStatusAsync(companyId, _currentUser.UserId));
}

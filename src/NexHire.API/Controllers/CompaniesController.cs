using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.DTOs.Company;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/v1/companies")]
public sealed class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companies;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateCompanyDto> _createValidator;
    private readonly IValidator<UpdateCompanyDto> _updateValidator;

    public CompaniesController(
        ICompanyService companies,
        ICurrentUserService currentUser,
        IValidator<CreateCompanyDto> createValidator,
        IValidator<UpdateCompanyDto> updateValidator)
    {
        _companies = companies;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // =========================================================
    // CREATE COMPANY
    // =========================================================

    [HttpPost]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> Create(
        CreateCompanyDto dto,
        CancellationToken cancellationToken)
    {
        var validation =
            await _createValidator.ValidateAsync(
                dto,
                cancellationToken);

        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                message = "Company validation failed.",
                errors = validation.Errors.Select(error => new
                {
                    field = error.PropertyName,
                    message = error.ErrorMessage
                })
            });
        }

        var company =
            await _companies.CreateCompanyAsync(
                _currentUser.UserId,
                dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = company.Id },
            company);
    }

    // =========================================================
    // GET MY COMPANIES
    // =========================================================

    [HttpGet("mine")]
    [HttpGet("me")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> GetMine()
    {
        var companies =
            await _companies.GetByOwnerAsync(
                _currentUser.UserId);

        return Ok(companies);
    }

    // =========================================================
    // GET COMPANY
    // =========================================================

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var company =
            await _companies.GetByIdAsync(id);

        if (company is null)
        {
            return NotFound(new
            {
                message = "Company not found."
            });
        }

        return Ok(company);
    }

    // =========================================================
    // UPDATE COMPANY
    // =========================================================

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateCompanyDto dto,
        CancellationToken cancellationToken)
    {
        var validation =
            await _updateValidator.ValidateAsync(
                dto,
                cancellationToken);

        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                message = "Company validation failed.",
                errors = validation.Errors.Select(error => new
                {
                    field = error.PropertyName,
                    message = error.ErrorMessage
                })
            });
        }

        var company =
            await _companies.UpdateCompanyAsync(
                id,
                _currentUser.UserId,
                dto);

        return Ok(company);
    }

    // =========================================================
    // UPLOAD VERIFICATION DOCUMENT
    // =========================================================

    [HttpPost("{id:guid}/verification-documents")]
    [HttpPost("{id:guid}/documents")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> UploadDocument(
        Guid id,
        UploadCompanyDocumentDto dto)
    {
        var document =
            await _companies.UploadDocumentAsync(
                id,
                _currentUser.UserId,
                dto);

        return Ok(document);
    }

    // =========================================================
    // VERIFY EMAIL - MVP SIMULATION
    // =========================================================

    [HttpPost("{id:guid}/verify-email")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> VerifyEmail(Guid id)
    {
        await _companies.MarkEmailVerifiedAsync(
            id,
            _currentUser.UserId);

        return Ok(new
        {
            message = "Company email verified successfully."
        });
    }

    // =========================================================
    // VERIFY PHONE - MVP SIMULATION
    // =========================================================

    [HttpPost("{id:guid}/verify-phone")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> VerifyPhone(Guid id)
    {
        await _companies.MarkPhoneVerifiedAsync(
            id,
            _currentUser.UserId);

        return Ok(new
        {
            message = "Company phone verified successfully."
        });
    }

    // =========================================================
    // SUBMIT VERIFICATION
    // =========================================================

    [HttpPost("{id:guid}/verification/submit")]
    [HttpPost("{id:guid}/submit-verification")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> SubmitVerification(
        Guid id,
        SubmitCompanyVerificationDto dto)
    {
        var result =
            await _companies.SubmitVerificationAsync(
                id,
                _currentUser.UserId,
                dto);

        return Ok(result);
    }

    // =========================================================
    // GET VERIFICATION STATUS
    // =========================================================

    [HttpGet("{id:guid}/verification")]
    [HttpGet("{id:guid}/verification-status")]
    [Authorize]
    public async Task<IActionResult> GetVerificationStatus(
        Guid id)
    {
        var result =
            await _companies.GetVerificationStatusAsync(
                id,
                _currentUser.UserId,
                _currentUser.IsInRole(RoleNames.Admin));

        return Ok(result);
    }

    // =========================================================
    // ADMIN REVIEW - EXISTING ROUTE
    // =========================================================

    [HttpPatch("{id:guid}/verification")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> ReviewVerification(
        Guid id,
        VerifyCompanyDto dto)
    {
        var company =
            await _companies.VerifyCompanyAsync(
                id,
                _currentUser.UserId,
                dto);

        return Ok(company);
    }
}
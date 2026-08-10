using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.DTOs.Company;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/v1/admin/companies")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class CompanyAdminController : ControllerBase
{
    private readonly ICompanyService _companies;
    private readonly ICurrentUserService _currentUser;

    public CompanyAdminController(
        ICompanyService companies,
        ICurrentUserService currentUser)
    {
        _companies = companies;
        _currentUser = currentUser;
    }

    // =========================================================
    // PENDING COMPANIES
    // =========================================================

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingCompanies()
    {
        var companies =
            await _companies.GetPendingVerificationAsync();

        return Ok(companies);
    }

    // =========================================================
    // COMPANY VERIFICATION DETAILS
    // =========================================================

    [HttpGet("{companyId:guid}/verification")]
    public async Task<IActionResult> GetVerification(
        Guid companyId)
    {
        var result =
            await _companies.GetVerificationStatusAsync(
                companyId,
                _currentUser.UserId,
                isAdmin: true);

        return Ok(result);
    }

    // =========================================================
    // REVIEW COMPANY
    // =========================================================

    [HttpPost("{companyId:guid}/review")]
    public async Task<IActionResult> ReviewCompany(
        Guid companyId,
        VerifyCompanyDto dto)
    {
        var company =
            await _companies.VerifyCompanyAsync(
                companyId,
                _currentUser.UserId,
                dto);

        return Ok(company);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.Common.Exceptions;
using NexHire.Application.DTOs.Company;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
public sealed class CompaniesController
    : ControllerBase
{
    private readonly ICompanyService _companies;
    private readonly ICurrentUserService _currentUser;

    public CompaniesController(
        ICompanyService companies,
        ICurrentUserService currentUser)
    {
        _companies = companies;
        _currentUser = currentUser;
    }

    [HttpGet("mine")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> Mine()
    {
        return Ok(
            await _companies.GetByOwnerAsync(
                _currentUser.UserId));
    }

    [HttpGet("{companyId:guid}")]
    public async Task<IActionResult> Get(
        Guid companyId)
    {
        var company =
            await _companies.GetByIdAsync(
                companyId);

        return company is null
            ? NotFound(
                new
                {
                    message =
                        "Company was not found."
                })
            : Ok(company);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> Create(
        CreateCompanyDto dto)
    {
        try
        {
            var company =
                await _companies.CreateCompanyAsync(
                    _currentUser.UserId,
                    dto);

            return CreatedAtAction(
                nameof(Get),
                new
                {
                    companyId =
                        company.Id
                },
                company);
        }
        catch (BusinessRuleException exception)
        {
            return Conflict(
                new { message = exception.Message });
        }
        catch (NexHire.Application.Common.Exceptions.ValidationException exception)
        {
            return BadRequest(
                new { message = exception.Message });
        }
    }

    [HttpPut("{companyId:guid}")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> Update(
        Guid companyId,
        UpdateCompanyDto dto)
    {
        try
        {
            return Ok(
                await _companies.UpdateCompanyAsync(
                    companyId,
                    _currentUser.UserId,
                    dto));
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
        catch (BusinessRuleException exception)
        {
            return BadRequest(
                new { message = exception.Message });
        }
    }
}

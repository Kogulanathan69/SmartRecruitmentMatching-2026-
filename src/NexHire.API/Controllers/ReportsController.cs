using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;
    private readonly ICurrentUserService _current;

    public ReportsController(
        IReportService reports,
        ICurrentUserService current)
    {
        _reports = reports;
        _current = current;
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet("platform")]
    public async Task<IActionResult> Platform()
    {
        return Ok(
            await _reports.GetPlatformSummaryAsync());
    }

    [Authorize(Roles = RoleNames.Employer)]
    [HttpGet("company/{companyId:guid}")]
    public async Task<IActionResult> Company(Guid companyId)
    {
        return Ok(
            await _reports.GetCompanyReportAsync(
                _current.UserId,
                companyId));
    }
}
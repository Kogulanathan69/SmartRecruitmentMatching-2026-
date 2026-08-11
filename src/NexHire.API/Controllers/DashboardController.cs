using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(
        IDashboardService service,
        ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    private Guid CurrentUserId =>
        _currentUser.UserId;

    [HttpGet("jobseeker")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> JobSeeker()
    {
        return Ok(
            await _service.GetJobSeekerAsync(
                CurrentUserId));
    }

    [HttpGet("employer")]
    [Authorize(Roles = RoleNames.Employer)]
    public async Task<IActionResult> Employer()
    {
        return Ok(
            await _service.GetEmployerAsync(
                CurrentUserId));
    }

    [HttpGet("admin")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Admin()
    {
        return Ok(
            await _service.GetAdminAsync(
                CurrentUserId));
    }
}

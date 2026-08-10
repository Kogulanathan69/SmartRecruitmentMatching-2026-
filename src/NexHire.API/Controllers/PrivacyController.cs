using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.DTOs.Privacy;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.JobSeeker)]
[Route("api/v1/privacy")]
public class PrivacyController : ControllerBase
{
    private readonly IPrivacyService _privacy;
    private readonly ICurrentUserService _current;

    public PrivacyController(
        IPrivacyService privacy,
        ICurrentUserService current)
    {
        _privacy = privacy;
        _current = current;
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> Get()
    {
        return Ok(
            await _privacy.GetPreferencesAsync(_current.UserId));
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> Update(
        PrivacyPreferencesDto dto)
    {
        return Ok(
            await _privacy.UpdatePreferencesAsync(
                _current.UserId,
                dto));
    }

    [HttpPost("deletion-requests")]
    public async Task<IActionResult> DeleteRequest(
        DeletionRequestDto dto)
    {
        return Ok(
            await _privacy.RequestDeletionAsync(
                _current.UserId,
                dto));
    }

    [HttpGet("deletion-requests")]
    public async Task<IActionResult> Requests()
    {
        return Ok(
            await _privacy.GetDeletionRequestsAsync(
                _current.UserId));
    }
}
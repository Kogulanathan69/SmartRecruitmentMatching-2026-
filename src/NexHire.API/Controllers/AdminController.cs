using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.DTOs.Admin;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/Admin/users")]
[Authorize(Roles = "Admin")]
public sealed class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMembers(CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetMembersAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMember(Guid id, CancellationToken cancellationToken)
    {
        var member = await adminService.GetMemberAsync(id, cancellationToken);
        return member is null
            ? NotFound(new { message = "Member not found." })
            : Ok(member);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMember(
        CreateMemberRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var member = await adminService.CreateMemberAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetMember), new { id = member.Id }, member);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMember(
        Guid id,
        UpdateMemberRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var member = await adminService.UpdateMemberAsync(id, request, cancellationToken);
            return member is null
                ? NotFound(new { message = "Member not found." })
                : Ok(member);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        UpdateUserStatusDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var succeeded = await adminService.ChangeStatusAsync(
                id,
                request.Status,
                GetCurrentUserId(),
                cancellationToken);

            return succeeded ? NoContent() : NotFound();
        }
        catch (Exception exception) when (
            exception is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(
        Guid id,
        AdminResetPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var succeeded = await adminService.ResetPasswordAsync(
                id,
                request,
                cancellationToken);

            return succeeded ? NoContent() : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeactivateMember(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var succeeded = await adminService.DeactivateAsync(
                id,
                GetCurrentUserId(),
                cancellationToken);

            return succeeded ? NoContent() : NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(claim, out var userId)
            ? userId
            : throw new UnauthorizedAccessException();
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.DTOs.Auth;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await authService.RegisterAsync(request, cancellationToken));
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

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        return response is null
            ? Unauthorized(new { message = "Invalid email or password." })
            : Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await authService.RefreshAsync(request.RefreshToken, cancellationToken);
        return response is null
            ? Unauthorized(new { message = "Invalid or expired refresh token." })
            : Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        var resetToken = await authService.ForgotPasswordAsync(
            request.Email,
            cancellationToken);

        return Ok(new
        {
            message = "If the email exists, reset instructions were created.",
            resetTokenForDevelopmentOnly = resetToken
        });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        var succeeded = await authService.ResetPasswordAsync(request, cancellationToken);
        return succeeded
            ? Ok(new { message = "Password reset successful." })
            : BadRequest(new { message = "Invalid/expired token or weak password." });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var succeeded = await authService.ChangePasswordAsync(
            userId.Value,
            request,
            cancellationToken);

        return succeeded
            ? Ok(new { message = "Password changed." })
            : BadRequest(new { message = "Password change failed." });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken)
    {
        var succeeded = await authService.LogoutAsync(
            request.RefreshToken,
            cancellationToken);

        return succeeded
            ? Ok(new { message = "Logged out." })
            : BadRequest(new { message = "Refresh token is invalid." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await authService.GetCurrentUserAsync(userId.Value, cancellationToken);
        return user is null ? Unauthorized() : Ok(user);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(claim, out var userId) ? userId : null;
    }
}

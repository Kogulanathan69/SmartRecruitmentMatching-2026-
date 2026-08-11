using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NexHire.Application.DTOs.Auth;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto dto, CancellationToken ct)
    {
        try
        {
            await auth.RegisterAsync(dto, ct);
            return Ok(new
            {
                message = "Registration created. Enter the 6-digit OTP sent to your email.",
                email = dto.Email.Trim().ToLowerInvariant(),
                requiresEmailVerification = true
            });
        }
        catch (ArgumentException x)
        {
            return BadRequest(new { message = x.Message });
        }
        catch (InvalidOperationException x) when (x.Message == "Email is already registered.")
        {
            return Conflict(new { message = x.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyEmailOtpRequestDto dto, CancellationToken ct)
        => await auth.VerifyEmailOtpAsync(dto, ct)
            ? Ok(new { message = "Email verified successfully. You can now sign in." })
            : BadRequest(new { message = "OTP is invalid, expired, or the maximum attempts were reached." });

    [AllowAnonymous]
    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(ResendEmailOtpRequestDto dto, CancellationToken ct)
        => await auth.ResendEmailOtpAsync(dto, ct)
            ? Ok(new { message = "If the account exists and still needs verification, a new OTP has been sent." })
            : BadRequest(new { message = "Please wait 60 seconds before requesting another OTP." });

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto, CancellationToken ct)
    {
        try
        {
            return await auth.LoginAsync(dto, ct) is { } r
                ? Ok(r)
                : Unauthorized(new { message = "Invalid email or password." });
        }
        catch (InvalidOperationException x)
        {
            return Unauthorized(new { message = x.Message, requiresEmailVerification = true, email = dto.Email.Trim().ToLowerInvariant() });
        }
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequestDto dto, CancellationToken ct)
        => await auth.RefreshAsync(dto.RefreshToken, ct) is { } r
            ? Ok(r)
            : Unauthorized(new { message = "Invalid or expired refresh token." });

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> Forgot(ForgotPasswordRequestDto dto, CancellationToken ct)
    {
        var token = await auth.ForgotPasswordAsync(dto.Email, ct);
        return Ok(new
        {
            message = "If the email exists, reset instructions were created.",
            resetTokenForDevelopmentOnly = token
        });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> Reset(ResetPasswordRequestDto dto, CancellationToken ct)
        => await auth.ResetPasswordAsync(dto, ct)
            ? Ok(new { message = "Password reset successful." })
            : BadRequest(new { message = "Invalid/expired token or weak password." });

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> Change(ChangePasswordRequestDto dto, CancellationToken ct)
        => GetUserId() is { } id && await auth.ChangePasswordAsync(id, dto, ct)
            ? Ok(new { message = "Password changed." })
            : BadRequest(new { message = "Password change failed." });

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequestDto dto, CancellationToken ct)
        => await auth.LogoutAsync(dto.RefreshToken, ct)
            ? Ok(new { message = "Logged out." })
            : BadRequest(new { message = "Refresh token is invalid." });

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
        => GetUserId() is { } id && await auth.GetCurrentUserAsync(id, ct) is { } u
            ? Ok(u)
            : Unauthorized();

    private Guid? GetUserId()
        => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id)
            ? id
            : null;
}
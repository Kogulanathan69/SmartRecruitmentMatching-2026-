using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.DTOs.Auth;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(
        IAuthService auth)
    {
        _auth = auth;
    }

    // ---------------------------------------------------------
    // REGISTER
    // ---------------------------------------------------------

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequestDto dto,
        CancellationToken ct)
    {
        try
        {
            await _auth.RegisterAsync(
                dto,
                ct);

            return Ok(new
            {
                message =
                    "Registration successful. " +
                    "OTP has been sent to your email. " +
                    "Please verify your email before login."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // ---------------------------------------------------------
    // VERIFY OTP
    // ---------------------------------------------------------

    [AllowAnonymous]
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(
        VerifyEmailOtpRequestDto dto,
        CancellationToken ct)
    {
        var verified =
            await _auth.VerifyEmailOtpAsync(
                dto,
                ct);

        if (!verified)
        {
            return BadRequest(new
            {
                message =
                    "Invalid or expired OTP."
            });
        }

        return Ok(new
        {
            message =
                "Email verified successfully. " +
                "You can now login."
        });
    }

    // ---------------------------------------------------------
    // RESEND OTP
    // ---------------------------------------------------------

    [AllowAnonymous]
    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(
        ResendEmailOtpRequestDto dto,
        CancellationToken ct)
    {
        var result =
            await _auth.ResendEmailOtpAsync(
                dto,
                ct);

        if (!result)
        {
            return BadRequest(new
            {
                message =
                    "Please wait before requesting another OTP."
            });
        }

        return Ok(new
        {
            message =
                "If the account exists and is not verified, " +
                "a new OTP has been sent."
        });
    }

    // ---------------------------------------------------------
    // LOGIN
    // ---------------------------------------------------------

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequestDto dto,
        CancellationToken ct)
    {
        try
        {
            var response =
                await _auth.LoginAsync(
                    dto,
                    ct);

            if (response is null)
            {
                return Unauthorized(new
                {
                    message =
                        "Invalid email or password."
                });
            }

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
    }

    // ---------------------------------------------------------
    // REFRESH TOKEN
    // ---------------------------------------------------------

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        RefreshTokenRequestDto dto,
        CancellationToken ct)
    {
        var response =
            await _auth.RefreshAsync(
                dto.RefreshToken,
                ct);

        if (response is null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid or expired refresh token."
            });
        }

        return Ok(response);
    }

    // ---------------------------------------------------------
    // FORGOT PASSWORD
    // ---------------------------------------------------------

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> Forgot(
        ForgotPasswordRequestDto dto,
        CancellationToken ct)
    {
        var token =
            await _auth.ForgotPasswordAsync(
                dto.Email,
                ct);

        return Ok(new
        {
            message =
                "If the email exists, " +
                "reset instructions were created.",

            resetTokenForDevelopmentOnly =
                token
        });
    }

    // ---------------------------------------------------------
    // RESET PASSWORD
    // ---------------------------------------------------------

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> Reset(
        ResetPasswordRequestDto dto,
        CancellationToken ct)
    {
        var result =
            await _auth.ResetPasswordAsync(
                dto,
                ct);

        if (!result)
        {
            return BadRequest(new
            {
                message =
                    "Invalid/expired token or weak password."
            });
        }

        return Ok(new
        {
            message =
                "Password reset successful."
        });
    }

    // ---------------------------------------------------------
    // CHANGE PASSWORD
    // ---------------------------------------------------------

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> Change(
        ChangePasswordRequestDto dto,
        CancellationToken ct)
    {
        var userId =
            GetUserId();

        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result =
            await _auth.ChangePasswordAsync(
                userId.Value,
                dto,
                ct);

        if (!result)
        {
            return BadRequest(new
            {
                message =
                    "Password change failed."
            });
        }

        return Ok(new
        {
            message =
                "Password changed."
        });
    }

    // ---------------------------------------------------------
    // LOGOUT
    // ---------------------------------------------------------

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        RefreshTokenRequestDto dto,
        CancellationToken ct)
    {
        var result =
            await _auth.LogoutAsync(
                dto.RefreshToken,
                ct);

        if (!result)
        {
            return BadRequest(new
            {
                message =
                    "Refresh token is invalid."
            });
        }

        return Ok(new
        {
            message =
                "Logged out."
        });
    }

    // ---------------------------------------------------------
    // CURRENT USER
    // ---------------------------------------------------------

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(
        CancellationToken ct)
    {
        var userId =
            GetUserId();

        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var user =
            await _auth.GetCurrentUserAsync(
                userId.Value,
                ct);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }

    // ---------------------------------------------------------
    // GET USER ID FROM JWT
    // ---------------------------------------------------------

    private Guid? GetUserId()
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ??
            User.FindFirstValue("sub");

        return Guid.TryParse(
            value,
            out var id)
                ? id
                : null;
    }
}
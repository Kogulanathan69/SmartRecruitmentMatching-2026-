using System.Security.Claims;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using NexHire.Application.DTOs.Auth;using NexHire.Application.Interfaces.Services;
namespace NexHire.API.Controllers;
[ApiController,Route("api/[controller]")]public sealed class AuthController(IAuthService auth):ControllerBase
{
 [AllowAnonymous,HttpPost("register")]public async Task<IActionResult>Register(RegisterRequestDto d,CancellationToken ct){try{return Ok(await auth.RegisterAsync(d,ct));}catch(ArgumentException x){return BadRequest(new{message=x.Message});}catch(InvalidOperationException x){return Conflict(new{message=x.Message});}}
 [AllowAnonymous,HttpPost("login")]public async Task<IActionResult>Login(LoginRequestDto d,CancellationToken ct)=>await auth.LoginAsync(d,ct) is{}r?Ok(r):Unauthorized(new{message="Invalid email or password."});
 [AllowAnonymous,HttpPost("refresh")]public async Task<IActionResult>Refresh(RefreshTokenRequestDto d,CancellationToken ct)=>await auth.RefreshAsync(d.RefreshToken,ct) is{}r?Ok(r):Unauthorized(new{message="Invalid or expired refresh token."});
 [AllowAnonymous,HttpPost("forgot-password")]public async Task<IActionResult>Forgot(ForgotPasswordRequestDto d,CancellationToken ct){var token=await auth.ForgotPasswordAsync(d.Email,ct);return Ok(new{message="If the email exists, reset instructions were created.",resetTokenForDevelopmentOnly=token});}
 [AllowAnonymous,HttpPost("reset-password")]public async Task<IActionResult>Reset(ResetPasswordRequestDto d,CancellationToken ct)=>await auth.ResetPasswordAsync(d,ct)?Ok(new{message="Password reset successful."}):BadRequest(new{message="Invalid/expired token or weak password."});
 [Authorize,HttpPost("change-password")]public async Task<IActionResult>Change(ChangePasswordRequestDto d,CancellationToken ct)=>GetUserId() is{}id&&await auth.ChangePasswordAsync(id,d,ct)?Ok(new{message="Password changed."}):BadRequest(new{message="Password change failed."});
 [Authorize,HttpPost("logout")]public async Task<IActionResult>Logout(RefreshTokenRequestDto d,CancellationToken ct)=>await auth.LogoutAsync(d.RefreshToken,ct)?Ok(new{message="Logged out."}):BadRequest(new{message="Refresh token is invalid."});
 [Authorize,HttpGet("me")]public async Task<IActionResult>Me(CancellationToken ct)=>GetUserId() is{}id&&await auth.GetCurrentUserAsync(id,ct) is{}u?Ok(u):Unauthorized();
 Guid? GetUserId()=>Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier)??User.FindFirstValue("sub"),out var id)?id:null;
}

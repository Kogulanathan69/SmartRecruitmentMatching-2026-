namespace NexHire.Application.DTOs.Auth;
public sealed record ResetPasswordRequestDto(string Token, string NewPassword, string ConfirmNewPassword);

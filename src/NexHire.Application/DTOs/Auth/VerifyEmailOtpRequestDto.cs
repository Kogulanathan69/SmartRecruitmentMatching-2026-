namespace NexHire.Application.DTOs.Auth;

public sealed record VerifyEmailOtpRequestDto(string Email, string Otp);
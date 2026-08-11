namespace NexHire.Application.DTOs.Auth;
public sealed record LoginResponseDto(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAtUtc, string Role, string FullName);

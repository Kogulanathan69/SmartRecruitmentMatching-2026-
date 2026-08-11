namespace NexHire.Application.DTOs.Auth;
public sealed record RegisterRequestDto(string Email, string Password, string ConfirmPassword, string FirstName, string LastName, string? PhoneNumber, string Role);

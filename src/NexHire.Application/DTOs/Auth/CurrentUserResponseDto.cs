namespace NexHire.Application.DTOs.Auth;
public sealed record CurrentUserResponseDto(Guid Id, string Email, string FullName, string Role, string Status);

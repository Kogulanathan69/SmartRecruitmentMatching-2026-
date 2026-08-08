namespace NexHire.Application.DTOs.Admin;
public sealed record CreateMemberRequestDto(string FirstName,string LastName,string Email,string? PhoneNumber,string Password,string ConfirmPassword,string Role);

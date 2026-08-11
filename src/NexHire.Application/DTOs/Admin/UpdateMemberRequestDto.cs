namespace NexHire.Application.DTOs.Admin;
public sealed record UpdateMemberRequestDto(string FirstName,string LastName,string? PhoneNumber,string Role);

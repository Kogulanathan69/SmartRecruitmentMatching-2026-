namespace NexHire.Application.DTOs.Admin;
public sealed record MemberResponseDto(Guid Id,string FirstName,string LastName,string FullName,string Email,string? PhoneNumber,string Role,string Status,DateTime CreatedAtUtc);

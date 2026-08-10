using NexHire.Domain.Enums;

namespace NexHire.Application.DTOs.Interview;

public sealed record InterviewResponse(
    Guid InterviewId,
    Guid ApplicationId,
    Guid CompanyId,
    Guid CandidateProfileId,
    string? CandidateName,
    string? CompanyName,
    string? JobTitle,
    DateTimeOffset ScheduledAtUtc,
    int DurationMinutes,
    InterviewMode Mode,
    string? MeetingLink,
    string? Location,
    string? ContactPhone,
    string? Notes,
    InterviewStatus Status,
    string? CancellationReason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? CompletedAtUtc);

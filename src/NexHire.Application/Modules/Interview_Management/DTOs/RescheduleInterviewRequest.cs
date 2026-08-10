using NexHire.Domain.Enums;

namespace NexHire.Application.DTOs.Interview;

public sealed record RescheduleInterviewRequest(
    DateTimeOffset ScheduledAtUtc,
    int DurationMinutes,
    InterviewMode Mode,
    string? MeetingLink,
    string? Location,
    string? ContactPhone,
    string? Notes);

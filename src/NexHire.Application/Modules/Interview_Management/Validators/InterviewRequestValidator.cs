using NexHire.Application.Common;
using NexHire.Application.DTOs.Interview;
using NexHire.Domain.Enums;

namespace NexHire.Application.Validators;

public static class InterviewRequestValidator
{
    public static void Validate(ScheduleInterviewRequest request, DateTimeOffset nowUtc)
    {
        if (request.ApplicationId == Guid.Empty)
            throw new Member5ValidationException("interview.application_required", "ApplicationId is required.");

        ValidateSchedule(
            request.ScheduledAtUtc,
            request.DurationMinutes,
            request.Mode,
            request.MeetingLink,
            request.Location,
            nowUtc);
    }

    public static void Validate(RescheduleInterviewRequest request, DateTimeOffset nowUtc) =>
        ValidateSchedule(
            request.ScheduledAtUtc,
            request.DurationMinutes,
            request.Mode,
            request.MeetingLink,
            request.Location,
            nowUtc);

    public static void ValidateScore(RecordInterviewScoreRequest request, Member5RulesOptions rules)
    {
        if (request.Score < rules.MinimumInterviewScore || request.Score > rules.MaximumInterviewScore)
        {
            throw new Member5ValidationException(
                "interview.score_out_of_range",
                $"Score must be between {rules.MinimumInterviewScore} and {rules.MaximumInterviewScore}.");
        }

        if (string.IsNullOrWhiteSpace(request.Feedback))
            throw new Member5ValidationException("interview.feedback_required", "Feedback is required.");
    }

    private static void ValidateSchedule(
        DateTimeOffset scheduledAtUtc,
        int durationMinutes,
        InterviewMode mode,
        string? meetingLink,
        string? location,
        DateTimeOffset nowUtc)
    {
        if (scheduledAtUtc <= nowUtc)
            throw new Member5ValidationException("interview.future_date_required", "Interview date and time must be in the future.");

        if (durationMinutes is < 15 or > 480)
            throw new Member5ValidationException("interview.invalid_duration", "Duration must be between 15 and 480 minutes.");

        if (!Enum.IsDefined(mode))
            throw new Member5ValidationException("interview.invalid_mode", "Unsupported interview mode.");

        if (mode == InterviewMode.Online &&
            (string.IsNullOrWhiteSpace(meetingLink) || !Uri.TryCreate(meetingLink, UriKind.Absolute, out _)))
        {
            throw new Member5ValidationException("interview.meeting_link_required", "Online interviews require a valid meeting link.");
        }

        if (mode == InterviewMode.Onsite && string.IsNullOrWhiteSpace(location))
            throw new Member5ValidationException("interview.location_required", "Onsite interviews require a location.");
    }
}

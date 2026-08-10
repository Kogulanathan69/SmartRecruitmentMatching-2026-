using NexHire.Domain.Enums;
using NexHire.Domain.Exceptions;

namespace NexHire.Domain.Entities;

public sealed class Interview
{
    private Interview()
    {
    }

    public Interview(
        Guid applicationId,
        Guid companyId,
        Guid candidateProfileId,
        DateTimeOffset scheduledAtUtc,
        int durationMinutes,
        InterviewMode mode,
        string? meetingLink,
        string? location,
        string? contactPhone,
        string? notes,
        Guid createdByUserId,
        DateTimeOffset nowUtc)
    {
        EnsureGuid(applicationId, nameof(applicationId));
        EnsureGuid(companyId, nameof(companyId));
        EnsureGuid(candidateProfileId, nameof(candidateProfileId));
        EnsureGuid(createdByUserId, nameof(createdByUserId));
        ValidateSchedule(scheduledAtUtc, durationMinutes, mode, meetingLink, location, nowUtc);

        InterviewId = Guid.NewGuid();
        ApplicationId = applicationId;
        CompanyId = companyId;
        CandidateProfileId = candidateProfileId;
        ScheduledAtUtc = scheduledAtUtc;
        DurationMinutes = durationMinutes;
        Mode = mode;
        MeetingLink = Normalize(meetingLink);
        Location = Normalize(location);
        ContactPhone = Normalize(contactPhone);
        Notes = Normalize(notes);
        Status = InterviewStatus.Scheduled;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public Guid InterviewId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid CandidateProfileId { get; private set; }
    public DateTimeOffset ScheduledAtUtc { get; private set; }
    public int DurationMinutes { get; private set; }
    public InterviewMode Mode { get; private set; }
    public string? MeetingLink { get; private set; }
    public string? Location { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? Notes { get; private set; }
    public InterviewStatus Status { get; private set; }
    public string? CancellationReason { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public void Reschedule(
        DateTimeOffset scheduledAtUtc,
        int durationMinutes,
        InterviewMode mode,
        string? meetingLink,
        string? location,
        string? contactPhone,
        string? notes,
        DateTimeOffset nowUtc)
    {
        EnsureActive();
        ValidateSchedule(scheduledAtUtc, durationMinutes, mode, meetingLink, location, nowUtc);

        ScheduledAtUtc = scheduledAtUtc;
        DurationMinutes = durationMinutes;
        Mode = mode;
        MeetingLink = Normalize(meetingLink);
        Location = Normalize(location);
        ContactPhone = Normalize(contactPhone);
        Notes = Normalize(notes);
        Status = InterviewStatus.Rescheduled;
        UpdatedAtUtc = nowUtc;
    }

    public void Cancel(string reason, DateTimeOffset nowUtc)
    {
        EnsureActive();
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new Member5DomainException("A cancellation reason is required.");
        }

        CancellationReason = reason.Trim();
        Status = InterviewStatus.Cancelled;
        UpdatedAtUtc = nowUtc;
    }

    public void Complete(DateTimeOffset nowUtc)
    {
        EnsureActive();
        Status = InterviewStatus.Completed;
        CompletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    private void EnsureActive()
    {
        if (Status is InterviewStatus.Completed or InterviewStatus.Cancelled)
        {
            throw new Member5DomainException($"Interview in {Status} status cannot be changed.");
        }
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
        {
            throw new Member5DomainException("Interview date and time must be in the future.");
        }

        if (durationMinutes is < 15 or > 480)
        {
            throw new Member5DomainException("Interview duration must be between 15 and 480 minutes.");
        }

        if (!Enum.IsDefined(mode))
        {
            throw new Member5DomainException("Unsupported interview mode.");
        }

        if (mode == InterviewMode.Online)
        {
            if (string.IsNullOrWhiteSpace(meetingLink) ||
                !Uri.TryCreate(meetingLink, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            {
                throw new Member5DomainException("Online interviews require a valid HTTP/HTTPS meeting link.");
            }
        }

        if (mode == InterviewMode.Onsite && string.IsNullOrWhiteSpace(location))
        {
            throw new Member5DomainException("Onsite interviews require a location.");
        }
    }

    private static void EnsureGuid(Guid value, string name)
    {
        if (value == Guid.Empty)
        {
            throw new Member5DomainException($"{name} is required.");
        }
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

using NexHire.Application.Common;
using NexHire.Application.DTOs.Interview;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Validators;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;
using NexHire.Domain.Exceptions;

namespace NexHire.Application.Services;

public sealed class InterviewService : IInterviewService
{
    private readonly IInterviewRepository _repository;
    private readonly IApplicationAccessReader _applications;
    private readonly ICurrentActor _actor;
    private readonly IMember5NotificationPublisher _notifications;
    private readonly Member5RulesOptions _rules;
    private readonly TimeProvider _timeProvider;

    public InterviewService(
        IInterviewRepository repository,
        IApplicationAccessReader applications,
        ICurrentActor actor,
        IMember5NotificationPublisher notifications,
        Member5RulesOptions rules,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _applications = applications;
        _actor = actor;
        _notifications = notifications;
        _rules = rules;
        _timeProvider = timeProvider;
    }

    public async Task<InterviewResponse> ScheduleAsync(
        ScheduleInterviewRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureCompanyActor();
        var now = UtcNow();
        InterviewRequestValidator.Validate(request, now);

        var application = await GetApplicationAsync(request.ApplicationId, cancellationToken);
        EnsureCompanyOwns(application.CompanyId);
        EnsureApplicationStatus(
            application.Status,
            _rules.AllowedInterviewApplicationStatuses,
            "interview.application_not_eligible",
            "The application is not eligible for interview scheduling.");

        try
        {
            var interview = new Interview(
                application.ApplicationId,
                application.CompanyId,
                application.CandidateProfileId,
                request.ScheduledAtUtc,
                request.DurationMinutes,
                request.Mode,
                request.MeetingLink,
                request.Location,
                request.ContactPhone,
                request.Notes,
                _actor.UserId,
                now);

            await _repository.AddAsync(interview, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            await _notifications.InterviewScheduledAsync(interview.InterviewId, interview.ApplicationId, cancellationToken);
            return Map(interview, application);
        }
        catch (Member5DomainException exception)
        {
            throw new Member5ValidationException("interview.domain_rule", exception.Message);
        }
    }

    public async Task<PagedResponse<InterviewResponse>> GetCompanyPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        EnsureCompanyActor();
        (page, pageSize) = NormalizePage(page, pageSize);
        var companyId = RequireCompanyId();
        var result = await _repository.GetCompanyPageAsync(companyId, page, pageSize, cancellationToken);
        return new PagedResponse<InterviewResponse>(
            await MapManyAsync(result.Items, cancellationToken), page, pageSize, result.TotalCount);
    }

    public async Task<PagedResponse<InterviewResponse>> GetCandidatePageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        EnsureCandidateActor();
        (page, pageSize) = NormalizePage(page, pageSize);
        var candidateId = RequireCandidateProfileId();
        var result = await _repository.GetCandidatePageAsync(candidateId, page, pageSize, cancellationToken);
        return new PagedResponse<InterviewResponse>(
            await MapManyAsync(result.Items, cancellationToken), page, pageSize, result.TotalCount);
    }

    public async Task<InterviewResponse> GetByIdAsync(
        Guid interviewId,
        CancellationToken cancellationToken = default)
    {
        var interview = await GetInterviewAsync(interviewId, cancellationToken);
        EnsureCanRead(interview);
        var application = await GetApplicationAsync(interview.ApplicationId, cancellationToken);
        return Map(interview, application);
    }

    public async Task<InterviewResponse> RescheduleAsync(
        Guid interviewId,
        RescheduleInterviewRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureCompanyActor();
        var now = UtcNow();
        InterviewRequestValidator.Validate(request, now);
        var interview = await GetInterviewAsync(interviewId, cancellationToken);
        EnsureCompanyOwns(interview.CompanyId);

        try
        {
            interview.Reschedule(
                request.ScheduledAtUtc,
                request.DurationMinutes,
                request.Mode,
                request.MeetingLink,
                request.Location,
                request.ContactPhone,
                request.Notes,
                now);
            await _repository.SaveChangesAsync(cancellationToken);
            await _notifications.InterviewChangedAsync(interview.InterviewId, "Rescheduled", cancellationToken);
            return Map(interview, await GetApplicationAsync(interview.ApplicationId, cancellationToken));
        }
        catch (Member5DomainException exception)
        {
            throw new Member5ConflictException("interview.invalid_transition", exception.Message);
        }
    }

    public async Task<InterviewResponse> CancelAsync(
        Guid interviewId,
        CancelInterviewRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureCompanyActor();
        var interview = await GetInterviewAsync(interviewId, cancellationToken);
        EnsureCompanyOwns(interview.CompanyId);

        try
        {
            interview.Cancel(request.Reason, UtcNow());
            await _repository.SaveChangesAsync(cancellationToken);
            await _notifications.InterviewChangedAsync(interview.InterviewId, "Cancelled", cancellationToken);
            return Map(interview, await GetApplicationAsync(interview.ApplicationId, cancellationToken));
        }
        catch (Member5DomainException exception)
        {
            throw new Member5ConflictException("interview.invalid_transition", exception.Message);
        }
    }

    public async Task<InterviewResponse> CompleteAsync(
        Guid interviewId,
        CancellationToken cancellationToken = default)
    {
        EnsureCompanyActor();
        var interview = await GetInterviewAsync(interviewId, cancellationToken);
        EnsureCompanyOwns(interview.CompanyId);

        try
        {
            interview.Complete(UtcNow());
            await _repository.SaveChangesAsync(cancellationToken);
            await _notifications.InterviewChangedAsync(interview.InterviewId, "Completed", cancellationToken);
            return Map(interview, await GetApplicationAsync(interview.ApplicationId, cancellationToken));
        }
        catch (Member5DomainException exception)
        {
            throw new Member5ConflictException("interview.invalid_transition", exception.Message);
        }
    }

    public async Task<InterviewScoreResponse> RecordScoreAsync(
        Guid interviewId,
        RecordInterviewScoreRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureCompanyActor();
        InterviewRequestValidator.ValidateScore(request, _rules);
        var interview = await GetInterviewAsync(interviewId, cancellationToken);
        EnsureCompanyOwns(interview.CompanyId);

        if (interview.Status != InterviewStatus.Completed)
            throw new Member5ConflictException("interview.not_completed", "Interview must be completed before scoring.");

        if (await _repository.HasEvaluatorScoreAsync(interviewId, _actor.UserId, cancellationToken))
            throw new Member5ConflictException("interview.score_exists", "This evaluator has already scored the interview.");

        var score = new InterviewScore(interviewId, _actor.UserId, request.Score, request.Feedback, UtcNow());
        await _repository.AddScoreAsync(score, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return new InterviewScoreResponse(
            score.InterviewScoreId,
            score.InterviewId,
            score.EvaluatorUserId,
            score.Score,
            score.Feedback,
            score.CreatedAtUtc);
    }

    private async Task<Interview> GetInterviewAsync(Guid interviewId, CancellationToken cancellationToken)
    {
        if (interviewId == Guid.Empty)
            throw new Member5ValidationException("interview.id_required", "InterviewId is required.");

        return await _repository.GetByIdAsync(interviewId, cancellationToken)
            ?? throw new Member5NotFoundException("interview.not_found", "Interview was not found.");
    }

    private async Task<ApplicationAccessSnapshot> GetApplicationAsync(Guid applicationId, CancellationToken cancellationToken) =>
        await _applications.GetAsync(applicationId, cancellationToken)
        ?? throw new Member5NotFoundException("application.not_found", "Application was not found.");

    private async Task<IReadOnlyList<InterviewResponse>> MapManyAsync(
        IReadOnlyList<Interview> interviews,
        CancellationToken cancellationToken)
    {
        var responses = new List<InterviewResponse>(interviews.Count);
        foreach (var interview in interviews)
        {
            var application = await _applications.GetAsync(interview.ApplicationId, cancellationToken);
            responses.Add(Map(interview, application));
        }
        return responses;
    }

    private static InterviewResponse Map(Interview interview, ApplicationAccessSnapshot? application) => new(
        interview.InterviewId,
        interview.ApplicationId,
        interview.CompanyId,
        interview.CandidateProfileId,
        application?.CandidateName,
        application?.CompanyName,
        application?.JobTitle,
        interview.ScheduledAtUtc,
        interview.DurationMinutes,
        interview.Mode,
        interview.MeetingLink,
        interview.Location,
        interview.ContactPhone,
        interview.Notes,
        interview.Status,
        interview.CancellationReason,
        interview.CreatedAtUtc,
        interview.UpdatedAtUtc,
        interview.CompletedAtUtc);

    private void EnsureCanRead(Interview interview)
    {
        if (IsRole(_actor.Role, _rules.CompanyRoles))
        {
            EnsureCompanyOwns(interview.CompanyId);
            return;
        }

        if (IsRole(_actor.Role, _rules.CandidateRoles))
        {
            if (_actor.CandidateProfileId != interview.CandidateProfileId)
                throw new Member5ForbiddenException("interview.forbidden", "You cannot access this interview.");
            return;
        }

        throw new Member5ForbiddenException("interview.role_forbidden", "Your role cannot access interviews.");
    }

    private void EnsureCompanyActor()
    {
        EnsureAuthenticated();
        if (!IsRole(_actor.Role, _rules.CompanyRoles))
            throw new Member5ForbiddenException("company.role_required", "A company role is required.");
        _ = RequireCompanyId();
    }

    private void EnsureCandidateActor()
    {
        EnsureAuthenticated();
        if (!IsRole(_actor.Role, _rules.CandidateRoles))
            throw new Member5ForbiddenException("candidate.role_required", "A candidate role is required.");
        _ = RequireCandidateProfileId();
    }

    private void EnsureAuthenticated()
    {
        if (!_actor.IsAuthenticated || _actor.UserId == Guid.Empty)
            throw new Member5ForbiddenException("auth.required", "Authentication is required.");
    }

    private Guid RequireCompanyId() => _actor.CompanyId
        ?? throw new Member5ForbiddenException("company.context_missing", "Company context is missing from the authenticated user.");

    private Guid RequireCandidateProfileId() => _actor.CandidateProfileId
        ?? throw new Member5ForbiddenException("candidate.context_missing", "Candidate profile context is missing from the authenticated user.");

    private void EnsureCompanyOwns(Guid companyId)
    {
        if (RequireCompanyId() != companyId)
            throw new Member5ForbiddenException("company.ownership_required", "The resource does not belong to your company.");
    }

    private static void EnsureApplicationStatus(string current, string[] allowed, string code, string message)
    {
        if (!allowed.Any(value => string.Equals(value, current, StringComparison.OrdinalIgnoreCase)))
            throw new Member5ConflictException(code, message);
    }

    private (int Page, int PageSize) NormalizePage(int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, Math.Max(1, _rules.MaximumPageSize));
        return (page, pageSize);
    }

    private static bool IsRole(string role, IEnumerable<string> allowed) =>
        allowed.Any(value => string.Equals(value, role, StringComparison.OrdinalIgnoreCase));

    private DateTimeOffset UtcNow() => _timeProvider.GetUtcNow();
}

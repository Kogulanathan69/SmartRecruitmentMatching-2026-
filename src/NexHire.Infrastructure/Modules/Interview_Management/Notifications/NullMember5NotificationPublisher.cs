using Microsoft.Extensions.Logging;
using NexHire.Application.Interfaces.Services;

namespace NexHire.Infrastructure.Notifications;

public sealed class NullMember5NotificationPublisher : IMember5NotificationPublisher
{
    private readonly ILogger<NullMember5NotificationPublisher> _logger;

    public NullMember5NotificationPublisher(ILogger<NullMember5NotificationPublisher> logger)
    {
        _logger = logger;
    }

    public Task InterviewScheduledAsync(Guid interviewId, Guid applicationId, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Interview {InterviewId} scheduled for application {ApplicationId}. Shared notification adapter is not configured.",
            interviewId,
            applicationId);
        return Task.CompletedTask;
    }

    public Task InterviewChangedAsync(Guid interviewId, string action, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Interview {InterviewId} action {Action}. Shared notification adapter is not configured.",
            interviewId,
            action);
        return Task.CompletedTask;
    }

    public Task OfferSentAsync(Guid offerId, Guid applicationId, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Offer {OfferId} sent for application {ApplicationId}. Shared notification adapter is not configured.",
            offerId,
            applicationId);
        return Task.CompletedTask;
    }

    public Task OfferRespondedAsync(Guid offerId, string action, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Offer {OfferId} action {Action}. Shared notification adapter is not configured.",
            offerId,
            action);
        return Task.CompletedTask;
    }
}

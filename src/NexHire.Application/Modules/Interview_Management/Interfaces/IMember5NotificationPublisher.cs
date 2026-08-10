namespace NexHire.Application.Interfaces.Services;

public interface IMember5NotificationPublisher
{
    Task InterviewScheduledAsync(Guid interviewId, Guid applicationId, CancellationToken cancellationToken);
    Task InterviewChangedAsync(Guid interviewId, string action, CancellationToken cancellationToken);
    Task OfferSentAsync(Guid offerId, Guid applicationId, CancellationToken cancellationToken);
    Task OfferRespondedAsync(Guid offerId, string action, CancellationToken cancellationToken);
}

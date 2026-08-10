using NexHire.Application.Common;
using NexHire.Application.DTOs.Interview;

namespace NexHire.Application.Interfaces.Services;

public interface IInterviewService
{
    Task<InterviewResponse> ScheduleAsync(ScheduleInterviewRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<InterviewResponse>> GetCompanyPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResponse<InterviewResponse>> GetCandidatePageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<InterviewResponse> GetByIdAsync(Guid interviewId, CancellationToken cancellationToken = default);
    Task<InterviewResponse> RescheduleAsync(Guid interviewId, RescheduleInterviewRequest request, CancellationToken cancellationToken = default);
    Task<InterviewResponse> CancelAsync(Guid interviewId, CancelInterviewRequest request, CancellationToken cancellationToken = default);
    Task<InterviewResponse> CompleteAsync(Guid interviewId, CancellationToken cancellationToken = default);
    Task<InterviewScoreResponse> RecordScoreAsync(Guid interviewId, RecordInterviewScoreRequest request, CancellationToken cancellationToken = default);
}

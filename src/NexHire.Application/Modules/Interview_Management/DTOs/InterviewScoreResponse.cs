namespace NexHire.Application.DTOs.Interview;

public sealed record InterviewScoreResponse(
    Guid InterviewScoreId,
    Guid InterviewId,
    Guid EvaluatorUserId,
    int Score,
    string Feedback,
    DateTimeOffset CreatedAtUtc);

using NexHire.Domain.Exceptions;

namespace NexHire.Domain.Entities;

public sealed class InterviewScore
{
    private InterviewScore()
    {
    }

    public InterviewScore(
        Guid interviewId,
        Guid evaluatorUserId,
        int score,
        string feedback,
        DateTimeOffset nowUtc)
    {
        if (interviewId == Guid.Empty)
        {
            throw new Member5DomainException("InterviewId is required.");
        }

        if (evaluatorUserId == Guid.Empty)
        {
            throw new Member5DomainException("EvaluatorUserId is required.");
        }

        if (string.IsNullOrWhiteSpace(feedback))
        {
            throw new Member5DomainException("Interview feedback is required.");
        }

        InterviewScoreId = Guid.NewGuid();
        InterviewId = interviewId;
        EvaluatorUserId = evaluatorUserId;
        Score = score;
        Feedback = feedback.Trim();
        CreatedAtUtc = nowUtc;
    }

    public Guid InterviewScoreId { get; private set; }
    public Guid InterviewId { get; private set; }
    public Guid EvaluatorUserId { get; private set; }
    public int Score { get; private set; }
    public string Feedback { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
}

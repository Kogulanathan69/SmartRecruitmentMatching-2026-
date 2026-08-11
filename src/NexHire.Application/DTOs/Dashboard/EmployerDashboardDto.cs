namespace NexHire.Application.DTOs.Dashboard;

public class EmployerDashboardDto
{
    public int TotalJobs { get; set; }
    public int PublishedJobs { get; set; }
    public int ClosedJobs { get; set; }

    public int TotalApplications { get; set; }
    public int SubmittedApplications { get; set; }
    public int UnderReviewApplications { get; set; }
    public int ShortlistedApplications { get; set; }
    public int WaitingListApplications { get; set; }

    public int PendingContactRequests { get; set; }
    public int UnreadNotifications { get; set; }

    public IReadOnlyList<RecentEmployerJobDto> RecentJobs
        { get; set; } = [];
}

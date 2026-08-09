namespace NexHire.Application.DTOs.Dashboard;

public class JobSeekerDashboardDto
{
    public int TotalApplications { get; set; }
    public int SubmittedApplications { get; set; }
    public int UnderReviewApplications { get; set; }
    public int ShortlistedApplications { get; set; }
    public int WaitingListApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int WithdrawnApplications { get; set; }
    public int PendingContactRequests { get; set; }
    public int UnreadNotifications { get; set; }

    public IReadOnlyList<RecentApplicationDto> RecentApplications
        { get; set; } = [];
}

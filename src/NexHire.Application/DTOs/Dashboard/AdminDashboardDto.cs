namespace NexHire.Application.DTOs.Dashboard;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalJobSeekers { get; set; }
    public int TotalEmployers { get; set; }
    public int TotalAdmins { get; set; }

    public int TotalCompanies { get; set; }
    public int TotalJobs { get; set; }
    public int PublishedJobs { get; set; }
    public int ClosedJobs { get; set; }

    public int TotalApplications { get; set; }
    public int PendingContactRequests { get; set; }

    public int TotalNotifications { get; set; }
    public int UnreadNotifications { get; set; }
}

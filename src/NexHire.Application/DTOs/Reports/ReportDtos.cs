namespace NexHire.Application.DTOs.Reports;
public class PlatformSummaryDto { public int Users { get; set; } public int Companies { get; set; } public int ActiveJobs { get; set; } public int Applications { get; set; } public int PendingVerifications { get; set; } }
public class CompanyReportDto { public Guid CompanyId { get; set; } public int Jobs { get; set; } public int ActiveJobs { get; set; } public int Applications { get; set; } public int Shortlisted { get; set; } public int Rejected { get; set; } }

using NexHire.Application.DTOs.Reports;
namespace NexHire.Application.Interfaces.Services;
public interface IReportService
{
    Task<PlatformSummaryDto> GetPlatformSummaryAsync();
    Task<CompanyReportDto> GetCompanyReportAsync(Guid employerUserId, Guid companyId);
}

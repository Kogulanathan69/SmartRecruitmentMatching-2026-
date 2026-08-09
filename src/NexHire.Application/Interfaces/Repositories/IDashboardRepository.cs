using NexHire.Application.DTOs.Dashboard;

namespace NexHire.Application.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<JobSeekerDashboardDto> GetJobSeekerAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<EmployerDashboardDto> GetEmployerAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<AdminDashboardDto> GetAdminAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

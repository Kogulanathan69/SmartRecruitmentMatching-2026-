using NexHire.Application.DTOs.Dashboard;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;

namespace NexHire.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;

    public DashboardService(
        IDashboardRepository repository)
    {
        _repository = repository;
    }

    public Task<JobSeekerDashboardDto> GetJobSeekerAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetJobSeekerAsync(
            userId,
            cancellationToken);
    }

    public Task<EmployerDashboardDto> GetEmployerAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetEmployerAsync(
            userId,
            cancellationToken);
    }

    public Task<AdminDashboardDto> GetAdminAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetAdminAsync(
            userId,
            cancellationToken);
    }
}

using NexHire.Application.Common;
using NexHire.Application.Interfaces.Services;

namespace NexHire.Infrastructure.Repositories;

public sealed class NotConfiguredApplicationAccessReader : IApplicationAccessReader
{
    public Task<ApplicationAccessSnapshot?> GetAsync(
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        throw new Member5DependencyException(
            "member5.application_reader_not_configured",
            "Member 4 must register an IApplicationAccessReader implementation before Interview/Offer endpoints can be used.");
    }
}

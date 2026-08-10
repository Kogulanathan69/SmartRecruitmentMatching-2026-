using Xunit;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;
using NexHire.Domain.Exceptions;

namespace NexHire.UnitTests.Member5;

public sealed class InterviewDomainTests
{
    [Fact]
    public void Schedule_OnlineWithoutLink_Throws()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.Throws<Member5DomainException>(() => new Interview(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), now.AddDays(1), 60,
            InterviewMode.Online, null, null, null, null, Guid.NewGuid(), now));
    }

    [Fact]
    public void Complete_ThenCancel_Throws()
    {
        var now = DateTimeOffset.UtcNow;
        var interview = new Interview(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), now.AddDays(1), 60,
            InterviewMode.Online, "https://meet.example/test", null, null, null, Guid.NewGuid(), now);
        interview.Complete(now.AddHours(1));
        Assert.Throws<Member5DomainException>(() => interview.Cancel("No longer needed", now.AddHours(2)));
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NexHire.Domain.Entities;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Notifications;
public class OutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxWorker> _logger;
    public OutboxWorker(IServiceScopeFactory scopeFactory, ILogger<OutboxWorker> logger){_scopeFactory=scopeFactory;_logger=logger;}
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope=_scopeFactory.CreateScope();
                var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var pending=await db.Set<OutboxMessage>().Where(x=>x.ProcessedAtUtc==null).OrderBy(x=>x.OccurredAtUtc).Take(50).ToListAsync(stoppingToken);
                foreach(var message in pending) message.ProcessedAtUtc=DateTime.UtcNow;
                if(pending.Count>0) await db.SaveChangesAsync(stoppingToken);
            }
            catch(Exception ex){_logger.LogError(ex,"Outbox worker iteration failed.");}
            await Task.Delay(TimeSpan.FromSeconds(10),stoppingToken);
        }
    }
}

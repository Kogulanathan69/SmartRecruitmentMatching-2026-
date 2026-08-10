using Microsoft.EntityFrameworkCore;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Domain.Entities;
using NexHire.Infrastructure.Data;
namespace NexHire.Infrastructure.Repositories;
public class AuditLogRepository:IAuditLogRepository
{
 private readonly AppDbContext _db;public AuditLogRepository(AppDbContext db)=>_db=db;public Task AddAsync(AuditLog log)=>_db.Set<AuditLog>().AddAsync(log).AsTask();public async Task<IReadOnlyList<AuditLog>>GetRecentAsync(int take=100)=>await _db.Set<AuditLog>().AsNoTracking().OrderByDescending(x=>x.CreatedAtUtc).Take(Math.Clamp(take,1,500)).ToListAsync();public Task<int>SaveChangesAsync()=>_db.SaveChangesAsync();
}

using NexHire.Domain.Entities;
namespace NexHire.Application.Interfaces.Repositories;
public interface IAuditLogRepository { Task AddAsync(AuditLog log); Task<IReadOnlyList<AuditLog>> GetRecentAsync(int take=100); Task<int> SaveChangesAsync(); }

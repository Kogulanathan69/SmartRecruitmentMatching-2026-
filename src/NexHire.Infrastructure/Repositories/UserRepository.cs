using Microsoft.EntityFrameworkCore;using NexHire.Application.Interfaces.Repositories;using NexHire.Domain.Entities;using NexHire.Infrastructure.Data;
namespace NexHire.Infrastructure.Repositories;
public sealed class UserRepository(AppDbContext db):IUserRepository
{
    public Task<User?>GetByEmailAsync(string email,CancellationToken ct=default)=>db.Users.Include(x=>x.RefreshTokens).SingleOrDefaultAsync(x=>x.NormalizedEmail==email,ct);
    public Task<User?>GetByIdAsync(Guid id,CancellationToken ct=default)=>db.Users.Include(x=>x.RefreshTokens).SingleOrDefaultAsync(x=>x.Id==id,ct);
    public Task<User?>GetByResetTokenHashAsync(string hash,CancellationToken ct=default)=>db.Users.Include(x=>x.RefreshTokens).SingleOrDefaultAsync(x=>x.PasswordResetTokenHash==hash,ct);
    public Task<RefreshToken?>GetRefreshTokenAsync(string hash,CancellationToken ct=default)=>db.RefreshTokens.Include(x=>x.User).ThenInclude(x=>x.RefreshTokens).SingleOrDefaultAsync(x=>x.TokenHash==hash,ct);
    public Task<bool>EmailExistsAsync(string email,CancellationToken ct=default)=>db.Users.AnyAsync(x=>x.NormalizedEmail==email,ct);
    public async Task AddUserAsync(User user,CancellationToken ct=default)=>await db.Users.AddAsync(user,ct);
    public async Task AddRefreshTokenAsync(RefreshToken token,CancellationToken ct=default)=>await db.RefreshTokens.AddAsync(token,ct);
    public async Task SaveChangesAsync(CancellationToken ct=default)=>await db.SaveChangesAsync(ct);
}

using Microsoft.EntityFrameworkCore;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Domain.Entities;
using NexHire.Infrastructure.Data;

namespace NexHire.Infrastructure.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(user => user.RefreshTokens)
            .SingleOrDefaultAsync(
                user => user.NormalizedEmail == normalizedEmail,
                cancellationToken);
    }

    public Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(user => user.RefreshTokens)
            .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .OrderByDescending(user => user.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<User?> GetByResetTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(user => user.RefreshTokens)
            .SingleOrDefaultAsync(
                user => user.PasswordResetTokenHash == tokenHash,
                cancellationToken);
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return dbContext.RefreshTokens
            .Include(token => token.User)
            .ThenInclude(user => user.RefreshTokens)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AnyAsync(
            user => user.NormalizedEmail == normalizedEmail,
            cancellationToken);
    }

    public async Task AddUserAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task AddRefreshTokenAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

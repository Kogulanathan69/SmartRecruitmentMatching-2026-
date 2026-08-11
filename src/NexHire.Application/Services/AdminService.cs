using NexHire.Application.DTOs.Admin;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Validators;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;

namespace NexHire.Application.Services;

public sealed class AdminService(
    IUserRepository users,
    IPasswordHasher passwordHasher) : IAdminService
{
    public async Task<IReadOnlyList<MemberResponseDto>> GetMembersAsync(
        CancellationToken cancellationToken = default)
    {
        var members = await users.GetAllAsync(cancellationToken);
        return members.Select(MapMember).ToList();
    }

    public async Task<MemberResponseDto?> GetMemberAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);
        return user is null ? null : MapMember(user);
    }

    public async Task<MemberResponseDto> CreateMemberAsync(
        CreateMemberRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validation = await new CreateMemberRequestValidator()
            .ValidateAsync(request, cancellationToken);
        EnsureValid(validation.Errors.Select(error => error.ErrorMessage), validation.IsValid);

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        if (await users.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            NormalizedEmail = normalizedEmail,
            PhoneNumber = request.PhoneNumber?.Trim(),
            Role = Enum.Parse<UserRole>(request.Role, ignoreCase: true),
            Status = UserStatus.Active,
            PasswordHash = passwordHasher.Hash(request.Password)
        };

        await users.AddUserAsync(user, cancellationToken);
        await users.SaveChangesAsync(cancellationToken);
        return MapMember(user);
    }

    public async Task<MemberResponseDto?> UpdateMemberAsync(
        Guid id,
        UpdateMemberRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validation = await new UpdateMemberRequestValidator()
            .ValidateAsync(request, cancellationToken);
        EnsureValid(validation.Errors.Select(error => error.ErrorMessage), validation.IsValid);

        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.Role = Enum.Parse<UserRole>(request.Role, ignoreCase: true);

        await users.SaveChangesAsync(cancellationToken);
        return MapMember(user);
    }

    public async Task<bool> ChangeStatusAsync(
        Guid id,
        string statusValue,
        Guid currentAdminId,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<UserStatus>(statusValue, ignoreCase: true, out var status))
        {
            throw new ArgumentException("Status must be Active, Suspended, or Disabled.");
        }

        if (id == currentAdminId && status != UserStatus.Active)
        {
            throw new InvalidOperationException("You cannot disable your own Admin account.");
        }

        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.Status = status;
        if (status != UserStatus.Active)
        {
            RevokeRefreshTokens(user);
        }

        await users.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(
        Guid id,
        AdminResetPasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validation = await new AdminResetPasswordRequestValidator()
            .ValidateAsync(request, cancellationToken);
        EnsureValid(validation.Errors.Select(error => error.ErrorMessage), validation.IsValid);

        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        RevokeRefreshTokens(user);
        await users.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<bool> DeactivateAsync(
        Guid id,
        Guid currentAdminId,
        CancellationToken cancellationToken = default)
    {
        return ChangeStatusAsync(
            id,
            UserStatus.Disabled.ToString(),
            currentAdminId,
            cancellationToken);
    }

    private static void EnsureValid(
        IEnumerable<string> errorMessages,
        bool isValid)
    {
        if (!isValid)
        {
            throw new ArgumentException(string.Join(" ", errorMessages));
        }
    }

    private static void RevokeRefreshTokens(User user)
    {
        foreach (var token in user.RefreshTokens.Where(token => token.RevokedAtUtc is null))
        {
            token.RevokedAtUtc = DateTime.UtcNow;
        }
    }

    private static MemberResponseDto MapMember(User user)
    {
        return new MemberResponseDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.Role.ToString(),
            user.Status.ToString(),
            user.CreatedAtUtc);
    }
}

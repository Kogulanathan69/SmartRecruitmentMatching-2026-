using NexHire.Domain.Entities;
namespace NexHire.Application.Interfaces.Services;
public interface ITokenService { (string Token, DateTime ExpiresAtUtc) CreateAccessToken(User user); string CreateSecureToken(); string HashToken(string token); }

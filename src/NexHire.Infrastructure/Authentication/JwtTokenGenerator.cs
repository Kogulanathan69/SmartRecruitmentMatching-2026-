using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NexHire.Application.Interfaces.Services;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Authentication;

public sealed class JwtTokenGenerator(IConfiguration configuration) : ITokenService
{
    public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(User user)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is missing.");

        if (Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException("JWT key must contain at least 32 bytes.");
        }

        var expiryMinutes = configuration.GetValue<int?>("Jwt:ExpiryMinutes") ?? 60;
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expiresAtUtc);
    }

    public string CreateSecureToken()
    {
        return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
    }

    public string HashToken(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
}

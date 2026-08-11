using NexHire.Application.Interfaces.Services;
namespace NexHire.Infrastructure.Authentication;
public sealed class PasswordHasher : IPasswordHasher { public string Hash(string password)=>BCrypt.Net.BCrypt.HashPassword(password,12);public bool Verify(string password,string passwordHash){try{return BCrypt.Net.BCrypt.Verify(password,passwordHash);}catch{return false;}} }

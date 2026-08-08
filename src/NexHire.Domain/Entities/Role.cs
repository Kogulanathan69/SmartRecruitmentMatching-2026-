using NexHire.Domain.Enums;
namespace NexHire.Domain.Entities;
public sealed class Role { public int Id { get; set; } public UserRole Name { get; set; } public ICollection<User> Users { get; set; } = new List<User>(); }

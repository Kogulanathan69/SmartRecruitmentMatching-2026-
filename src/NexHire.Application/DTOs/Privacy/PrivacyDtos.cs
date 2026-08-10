namespace NexHire.Application.DTOs.Privacy;
public class PrivacyPreferencesDto { public bool IsProfilePublic { get; set; } public bool IsOpenToWork { get; set; } }
public class DeletionRequestDto { public string? Reason { get; set; } }
public class DeletionRequestResponseDto { public Guid Id { get; set; } public string Status { get; set; } = string.Empty; public string? Reason { get; set; } public DateTime SubmittedAtUtc { get; set; } }

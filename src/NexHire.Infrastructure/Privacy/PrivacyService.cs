using Microsoft.EntityFrameworkCore;
using NexHire.Application.Common.Exceptions;
using NexHire.Application.DTOs.Privacy;
using NexHire.Application.Interfaces.Services;
using NexHire.Domain.Entities;
using NexHire.Infrastructure.Data;
namespace NexHire.Infrastructure.Privacy;
public class PrivacyService:IPrivacyService
{
 private readonly AppDbContext _db;public PrivacyService(AppDbContext db)=>_db=db;
 public async Task<PrivacyPreferencesDto> GetPreferencesAsync(Guid userId){var p=await _db.Set<JobSeekerProfile>().AsNoTracking().FirstOrDefaultAsync(x=>x.UserId==userId)??throw new NotFoundException("Candidate profile not found.");return new(){IsProfilePublic=p.IsProfilePublic,IsOpenToWork=p.IsOpenToWork};}
 public async Task<PrivacyPreferencesDto> UpdatePreferencesAsync(Guid userId,PrivacyPreferencesDto dto){var p=await _db.Set<JobSeekerProfile>().FirstOrDefaultAsync(x=>x.UserId==userId)??throw new NotFoundException("Candidate profile not found.");p.IsProfilePublic=dto.IsProfilePublic;p.IsOpenToWork=dto.IsOpenToWork;p.UpdatedAt=DateTime.UtcNow;_db.Set<AuditLog>().Add(new AuditLog{ActorUserId=userId,Action="PrivacyPreferencesUpdated",EntityType="JobSeekerProfile",EntityId=p.Id});await _db.SaveChangesAsync();return dto;}
 public async Task<DeletionRequestResponseDto> RequestDeletionAsync(Guid userId,DeletionRequestDto dto){var existing=await _db.Set<PrivacyDeletionRequest>().FirstOrDefaultAsync(x=>x.UserId==userId&&(x.Status=="Submitted"||x.Status=="UnderReview"));if(existing!=null)return Map(existing);var r=new PrivacyDeletionRequest{UserId=userId,Reason=dto.Reason?.Trim()};_db.Add(r);_db.Set<AuditLog>().Add(new AuditLog{ActorUserId=userId,Action="DeletionRequested",EntityType="PrivacyDeletionRequest",EntityId=r.Id});await _db.SaveChangesAsync();return Map(r);}
 public async Task<IReadOnlyList<DeletionRequestResponseDto>> GetDeletionRequestsAsync(Guid userId)=>await _db.Set<PrivacyDeletionRequest>().AsNoTracking().Where(x=>x.UserId==userId).OrderByDescending(x=>x.SubmittedAtUtc).Select(x=>new DeletionRequestResponseDto{Id=x.Id,Status=x.Status,Reason=x.Reason,SubmittedAtUtc=x.SubmittedAtUtc}).ToListAsync();

 public async Task<DeletionRequestResponseDto> ReviewDeletionAsync(Guid adminUserId,Guid requestId,string status){var allowed=new[]{"UnderReview","Approved","Rejected","Completed"};if(!allowed.Contains(status,StringComparer.OrdinalIgnoreCase))throw new ValidationException("Status must be UnderReview, Approved, Rejected, or Completed.");var r=await _db.Set<PrivacyDeletionRequest>().FirstOrDefaultAsync(x=>x.Id==requestId)??throw new NotFoundException("Deletion request not found.");r.Status=allowed.First(x=>x.Equals(status,StringComparison.OrdinalIgnoreCase));r.ReviewedAtUtc=DateTime.UtcNow;r.ReviewedByUserId=adminUserId;_db.Set<AuditLog>().Add(new AuditLog{ActorUserId=adminUserId,Action="DeletionRequestReviewed",EntityType="PrivacyDeletionRequest",EntityId=r.Id,Details=r.Status});await _db.SaveChangesAsync();return Map(r);}
 private static DeletionRequestResponseDto Map(PrivacyDeletionRequest x)=>new(){Id=x.Id,Status=x.Status,Reason=x.Reason,SubmittedAtUtc=x.SubmittedAtUtc};
}

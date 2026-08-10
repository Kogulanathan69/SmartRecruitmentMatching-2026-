using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.Interfaces.Repositories;
namespace NexHire.API.Controllers;
[ApiController][Authorize(Roles=RoleNames.Admin)][Route("api/v1/admin/audit")]
public class AuditController:ControllerBase
{
 private readonly IAuditLogRepository _audit;public AuditController(IAuditLogRepository audit)=>_audit=audit;[HttpGet]public async Task<IActionResult>Get([FromQuery]int take=100)=>Ok(await _audit.GetRecentAsync(take));
}

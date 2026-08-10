using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.Interfaces.Services;
namespace NexHire.API.Controllers;
[ApiController][Authorize(Roles=RoleNames.Admin)][Route("api/v1/admin/privacy")]
public class AdminPrivacyController:ControllerBase
{
 private readonly IPrivacyService _privacy;private readonly ICurrentUserService _current;public AdminPrivacyController(IPrivacyService privacy,ICurrentUserService current){_privacy=privacy;_current=current;}
 [HttpPatch("deletion-requests/{id:guid}/{status}")]public async Task<IActionResult>Review(Guid id,string status)=>Ok(await _privacy.ReviewDeletionAsync(_current.UserId,id,status));
}

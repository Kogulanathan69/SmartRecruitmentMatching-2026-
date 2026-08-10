using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(
        INotificationService service,
        ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    private Guid CurrentUserId =>
        _currentUser.UserId;

    [HttpGet]
    public async Task<IActionResult> Mine()
    {
        return Ok(
            await _service.GetMineAsync(
                CurrentUserId));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount()
    {
        var count =
            await _service.GetUnreadCountAsync(
                CurrentUserId);

        return Ok(
            new
            {
                unreadCount = count
            });
    }

    [HttpPatch("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkRead(
        Guid notificationId)
    {
        try
        {
            return Ok(
                await _service.MarkReadAsync(
                    notificationId,
                    CurrentUserId));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(
                new
                {
                    message = exception.Message
                });
        }
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var count =
            await _service.MarkAllReadAsync(
                CurrentUserId);

        return Ok(
            new
            {
                markedRead = count
            });
    }
}

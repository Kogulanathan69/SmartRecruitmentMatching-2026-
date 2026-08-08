using System.Security.Claims;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? CurrentPrincipal =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        CurrentPrincipal?.Identity?.IsAuthenticated ?? false;

    public Guid UserId
    {
        get
        {
            var value =
                CurrentPrincipal?.FindFirstValue(
                    ClaimTypes.NameIdentifier)
                ?? CurrentPrincipal?.FindFirstValue("sub");

            if (Guid.TryParse(value, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException(
                "Authenticated user id was not found.");
        }
    }

    public string? Email =>
        CurrentPrincipal?.FindFirstValue(ClaimTypes.Email)
        ?? CurrentPrincipal?.FindFirstValue("email");

    public bool IsInRole(string role)
    {
        return CurrentPrincipal?.IsInRole(role) ?? false;
    }
}
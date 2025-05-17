using CRM.FileStorage.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CRM.FileStorage.Infrastructure.Services;

public class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public Guid? UserId
    {
        get
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst("Uid");
            if (userIdClaim == null) return null;
            return Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
        }
    }

    public string? Email => httpContextAccessor.HttpContext?.User?.FindFirst("Email")?.Value;

    public string? IpAddress => httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
}
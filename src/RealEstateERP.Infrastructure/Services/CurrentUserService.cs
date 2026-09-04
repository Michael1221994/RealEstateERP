using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RealEstateERP.Core.Contract.Service;

namespace RealEstateERP.Infrastructure.Services;

/// <summary>
/// Reads the authenticated actor's id from the JWT (UserID claim, with the standard
/// nameidentifier claim as fallback). No HTTP context (seeding, migration, background
/// work) yields null.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var value = principal?.FindFirst(ICurrentUserService.UserIdClaimType)?.Value
                        ?? principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
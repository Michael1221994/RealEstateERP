using System.Security.Claims;

namespace RealEstateERP.API.Extensions;

/// <summary>
/// Identity always comes from the token, never from a request body value.
/// The JWT (see AccessTokenService) carries the user's Guid in the "UserID"
/// claim plus the standard sub / nameidentifier claims as fallbacks.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public const string UserIdClaimType = "UserID";

    /// <summary>
    /// Returns the authenticated user's id parsed from the token claims,
    /// or null when the claim is missing or not a valid Guid.
    /// </summary>
    public static Guid? GetCurrentUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(UserIdClaimType)?.Value
                    ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}

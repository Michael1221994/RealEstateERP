namespace RealEstateERP.Core.Contract.Service;

/// <summary>
/// Resolves the authenticated actor's id for the current request.
/// Identity always comes from the JWT claim, never from request bodies (see decisions D-018).
/// Returns null when there is no authenticated request (e.g. seeding, background work).
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Claim type that carries the user's Guid in the JWT (see AccessTokenService).</summary>
    public const string UserIdClaimType = "UserID";

    Guid? UserId { get; }
}
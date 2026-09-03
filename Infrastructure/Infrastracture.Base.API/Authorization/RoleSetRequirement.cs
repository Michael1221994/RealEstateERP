using Microsoft.AspNetCore.Authorization;

namespace Infrastracture.Base.API.Authorization
{
    // Authorizes based on a caller-supplied set of roles resolved at policy-registration time
    // (e.g. from an ApprovalFlow<TStatus>.AllRoles), instead of a role list hardcoded on the
    // [Authorize] attribute that has to be kept in sync by hand.
    public class RoleSetRequirement : IAuthorizationRequirement
    {
        public IReadOnlyCollection<string> AllowedRoles { get; }

        public RoleSetRequirement(IReadOnlyCollection<string> allowedRoles)
        {
            AllowedRoles = allowedRoles;
        }
    }
}

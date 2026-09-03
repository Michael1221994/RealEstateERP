using Microsoft.AspNetCore.Authorization;

namespace Infrastracture.Base.API.Authorization
{
    public class RoleSetAuthorizationHandler : AuthorizationHandler<RoleSetRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleSetRequirement requirement)
        {
            if (requirement.AllowedRoles.Any(context.User.IsInRole))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}

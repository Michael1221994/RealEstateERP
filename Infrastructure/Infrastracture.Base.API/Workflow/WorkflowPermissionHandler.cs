using Infrastracture.Base.WorkflowOptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Security.Claims;


namespace Infrastracture.Base.API.Workflow
{
    public class WorkflowPermissionHandler : AuthorizationHandler<WorkflowPermissionRequirement>
    {
        private const string DefaultOrgKey = "PSSSA";
        private readonly WorkflowOption _options;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkflowPermissionHandler(IOptions<WorkflowOption> options, IHttpContextAccessor httpContextAccessor)
        {
            _options = options.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            WorkflowPermissionRequirement requirement)
        {
            // var orgKey = ResolveOrgKey(context);
            var orgKey = DefaultOrgKey;

            if (!TryGetOrgWorkflows(_options, orgKey, out var orgWorkflows))
                return Task.CompletedTask;

            if (!orgWorkflows.TryGetValue(requirement.WorkflowName, out var workflow))
                return Task.CompletedTask;

            if (!workflow.Permissions.TryGetValue(requirement.Permission, out var allowedRoles))
                return Task.CompletedTask;

            var userRoles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role || // Standard .NET Role Type
                            c.Type == "role" ||          // Common JWT Role Claim Type
                            c.Type == "roles")           // Another common JWT Role Claim Type
                .Select(c => c.Value)
                .ToHashSet();

            if (allowedRoles.Any(r => userRoles.Contains(r)))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }

        private string ResolveOrgKey(AuthorizationHandlerContext context)
        {
            // Prefer a token/claim driven org if available.
            var claimOrg = FindFirstValue(context.User,
                "org_key",
                "orgKey",
                "OrgKey",
                "org_category",
                "OrgCategory",
                "organization",
                "organization_key",
                "organizationKey",
                "tenant",
                "tenant_id");

            var headerOrg = TryGetHeaderValue(context, "X-Org-Key") ??
                           TryGetHeaderValue(context, "X-Workflow-Org");

            var orgFromIssuer = InferOrgFromIssuer(context.User);

            var requested = FirstNonEmpty(claimOrg, headerOrg, orgFromIssuer);
            if (string.IsNullOrWhiteSpace(requested))
            {
                return DefaultOrgKey;
            }

            // Validate against configured org keys; fallback if unknown.
            return HasOrgKey(_options, requested) ? requested : DefaultOrgKey;
        }

        private static string? FindFirstValue(ClaimsPrincipal user, params string[] claimTypes)
        {
            foreach (var claimType in claimTypes)
            {
                var value = user.FindFirst(claimType)?.Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        private static string? FirstNonEmpty(params string?[] values)
            => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

        private static string? InferOrgFromIssuer(ClaimsPrincipal user)
        {
            var issuer = user.FindFirst("iss")?.Value ?? user.FindFirst(ClaimTypes.Uri)?.Value;
            if (string.IsNullOrWhiteSpace(issuer))
            {
                return null;
            }

            // Best-effort heuristic: if issuer contains org name.
            if (issuer.Contains("poessa", StringComparison.OrdinalIgnoreCase))
            {
                return "POESSA";
            }

            if (issuer.Contains("psssa", StringComparison.OrdinalIgnoreCase))
            {
                return "PSSSA";
            }

            return null;
        }

        private static bool HasOrgKey(WorkflowOption options, string orgKey)
        {
            if (options?.Workflows == null || options.Workflows.Count == 0)
            {
                return false;
            }

            return options.Workflows.Keys.Any(k => string.Equals(k, orgKey, StringComparison.OrdinalIgnoreCase));
        }

        private static bool TryGetOrgWorkflows(
            WorkflowOption options,
            string orgKey,
            out Dictionary<string, WorkflowDefinition> orgWorkflows)
        {
            orgWorkflows = new Dictionary<string, WorkflowDefinition>(StringComparer.OrdinalIgnoreCase);
            if (options?.Workflows == null || options.Workflows.Count == 0)
            {
                return false;
            }

            foreach (var kvp in options.Workflows)
            {
                if (string.Equals(kvp.Key, orgKey, StringComparison.OrdinalIgnoreCase))
                {
                    orgWorkflows = kvp.Value;
                    return true;
                }
            }

            return false;
        }

        private string? TryGetHeaderValue(AuthorizationHandlerContext context, string headerName)
        {
            // Prefer the resource HttpContext when available.
            HttpContext? httpContext = context.Resource switch
            {
                HttpContext ctx => ctx,
                AuthorizationFilterContext filterCtx => filterCtx.HttpContext,
                _ => _httpContextAccessor.HttpContext
            };

            if (httpContext == null)
            {
                return null;
            }

            if (httpContext.Request.Headers.TryGetValue(headerName, out var values))
            {
                var value = values.FirstOrDefault();
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }

            return null;
        }
    }
}
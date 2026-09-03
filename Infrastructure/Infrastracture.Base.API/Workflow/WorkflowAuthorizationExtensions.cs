using Microsoft.AspNetCore.Authorization;
using Infrastracture.Base.WorkflowOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastracture.Base.API.Workflow
{
    public static class WorkflowAuthorizationExtensions
    {
        public static IServiceCollection AddWorkflowAuthorization(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register the handler once, shared across all policies
            services.AddSingleton<IAuthorizationHandler, WorkflowPermissionHandler>();

            // Bind from configuration root so WorkflowOption.Workflows maps to the "Workflows" section.
            var workflowOptions = configuration.Get<WorkflowOption>();

            if (workflowOptions?.Workflows == null || workflowOptions.Workflows.Count == 0)
            {
                return services;
            }

            services.AddAuthorization(options =>
            {
                var added = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Workflow config is org-keyed (e.g., POESSA/PSSSA). Build policies from the union of all org configs.
                foreach (var orgWorkflows in workflowOptions.Workflows.Values)
                {
                    foreach (var (workflowName, workflowDefinition) in orgWorkflows)
                    {
                        if (workflowDefinition?.Permissions == null || workflowDefinition.Permissions.Count == 0)
                        {
                            continue;
                        }

                        foreach (var permissionName in workflowDefinition.Permissions.Keys)
                        {
                            var policyName = $"{workflowName}.{permissionName}";
                            if (!added.Add(policyName))
                            {
                                continue;
                            }

                            options.AddPolicy(policyName, policy =>
                                policy.AddRequirements(
                                    new WorkflowPermissionRequirement(workflowName, permissionName)));
                        }
                    }
                }
            });

            return services;
        }
    }
}
using Microsoft.AspNetCore.Authorization;

namespace Infrastracture.Base.API.Workflow
{
    public class WorkflowPermissionRequirement : IAuthorizationRequirement
    {
        public string WorkflowName { get; }
        public string Permission { get; }

        public WorkflowPermissionRequirement(string workflowName, string permission)
        {
            WorkflowName = workflowName;
            Permission = permission;
        }
    }

}
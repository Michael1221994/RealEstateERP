namespace Infrastracture.Base.WorkflowOptions
{
    public class WorkflowOption
    {
        // keyed by org (poessa and psssa) then by workflow name like InvestmentPolicy
        public Dictionary<string, Dictionary<string, WorkflowDefinition>> Workflows { get; set; } = new();
    }

    public class WorkflowDefinition
    {
        // permission matrix "who can do what"
        public Dictionary<string, List<string>> Permissions { get; set; } = new();

        // approval chains per operation type
        public List<WorkflowStep> ApprovalSteps { get; set; } = new();
    }

    public class WorkflowStep
    {
        public List<string> Role { get; set; } = new();

        // to status
        public string OnApprove { get; set; } = "";
        public string OnReject { get; set; } = "";
        public string OnReturn { get; set; } = "";

    }
}
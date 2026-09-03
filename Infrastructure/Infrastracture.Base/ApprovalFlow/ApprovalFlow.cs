using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastracture.Base.ApprovalFlow
{
    public enum ApprovalAction
    {
        Approve,
        Reject,
        Return
    }

    // One rule per status a record can be actioned from: who may act, and what each action leads to.
    public class ApprovalStepRule<TStatus> where TStatus : struct, Enum
    {
        public required TStatus Status { get; init; }
        public string StepName { get; init; } = "";
        public required IReadOnlyCollection<string> Roles { get; init; }
        public TStatus? OnApprove { get; init; }
        public TStatus? OnReject { get; init; }
        public TStatus? OnReturn { get; init; }

        public TStatus? Resolve(ApprovalAction action) => action switch
        {
            ApprovalAction.Approve => OnApprove,
            ApprovalAction.Reject => OnReject,
            ApprovalAction.Return => OnReturn,
            _ => null
        };
    }

    public record ApprovalEvaluationResult<TStatus> where TStatus : struct, Enum
    {
        public bool IsAuthorized { get; init; }
        public bool IsOverride { get; init; }
        public string? MatchedRole { get; init; }
        public TStatus? FromStatus { get; init; }
        public TStatus? ToStatus { get; init; }
        public string? FailureReason { get; init; }

        public static ApprovalEvaluationResult<TStatus> Success(TStatus from, TStatus to, string matchedRole, bool isOverride) =>
            new() { IsAuthorized = true, FromStatus = from, ToStatus = to, MatchedRole = matchedRole, IsOverride = isOverride };

        public static ApprovalEvaluationResult<TStatus> Failure(string reason) =>
            new() { IsAuthorized = false, FailureReason = reason };
    }

    // Single source of truth for "who can act at status X, and what happens next" for a given
    // status enum. Feature code declares one ApprovalFlow<TStatus> with its step rules and calls
    // Evaluate/GetActionableStatuses instead of hand-writing role-matching/next-status switches.
    public class ApprovalFlow<TStatus> where TStatus : struct, Enum
    {
        // The role that can act at any step, bypassing step-role checks (see Evaluate/
        // GetActionableStatuses) - declared once here instead of by every feature's FooApprovalFlow,
        // since they'd all otherwise redeclare the same "Administrator can do everything" rule.
        public string SuperUserRole => "Administrator";

        private readonly IReadOnlyDictionary<TStatus, ApprovalStepRule<TStatus>> _steps;

        public ApprovalFlow(IEnumerable<ApprovalStepRule<TStatus>> steps)
        {
            _steps = steps.ToDictionary(s => s.Status);
        }

        public bool TryGetStep(TStatus status, out ApprovalStepRule<TStatus>? step) => _steps.TryGetValue(status, out step);

        // Every role that is allowed to act on this flow at some step, plus the super user. The set
        // to use for "can this caller reach any approval endpoint of this flow at all" - coarse
        // endpoint-level authorization should read this instead of hardcoding a role list that has
        // to be kept in sync with the step rules by hand.
        public IReadOnlyCollection<string> AllRoles =>
            _steps.Values.SelectMany(s => s.Roles).Append(SuperUserRole).Distinct().ToArray();

        public ApprovalEvaluationResult<TStatus> Evaluate(TStatus currentStatus, ApprovalAction action, IReadOnlyCollection<string> callerRoles)
        {
            if (!_steps.TryGetValue(currentStatus, out var step))
                return ApprovalEvaluationResult<TStatus>.Failure($"No approval step is configured for status '{currentStatus}'.");

            var isOverride = callerRoles.Contains(SuperUserRole);
            var matchedRole = isOverride ? SuperUserRole : step.Roles.FirstOrDefault(callerRoles.Contains);

            if (matchedRole == null)
                return ApprovalEvaluationResult<TStatus>.Failure("You are not authorized to act on this record at its current status.");

            var next = step.Resolve(action);
            if (next is null)
                return ApprovalEvaluationResult<TStatus>.Failure($"Action '{action}' is not valid from status '{currentStatus}'.");

            return ApprovalEvaluationResult<TStatus>.Success(currentStatus, next.Value, matchedRole, isOverride);
        }

        // Statuses the caller's roles may currently act on - drives "pending my approval" queries
        // without a per-feature role->status switch. Unrecognized roles simply resolve to an empty
        // set rather than falling back to any particular status.
        public IReadOnlyCollection<TStatus> GetActionableStatuses(IReadOnlyCollection<string> callerRoles)
        {
            if (callerRoles.Contains(SuperUserRole))
                return _steps.Keys.ToArray();

            return _steps.Values
                .Where(s => s.Roles.Any(callerRoles.Contains))
                .Select(s => s.Status)
                .ToArray();
        }
    }
}

using System.Collections.Immutable;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Core.Policies;

public sealed class PolicyEvaluator
{
    public PolicyEvaluation Evaluate(DiffResult diff, Snapshot current, PolicyOptions options)
    {
        List<PolicyViolation> violations = [];
        HashSet<string> ignored = new(options.IgnoredPatterns, StringComparer.Ordinal);
        HashSet<string> allowed = new(options.AllowedPatterns, StringComparer.Ordinal);
        if (options.FailOnIncompleteScan && !current.Scan.IsComplete)
        {
            Add("RKP001", "Current snapshot is incomplete.");
        }

        foreach (DiffChange change in diff.Changes)
        {
            if (change.Kind is ChangeKind.PatternAdded or ChangeKind.PatternRemoved or ChangeKind.PatternCountChanged && ignored.Contains(change.Subject))
            {
                continue;
            }

            if (change.Kind == ChangeKind.PatternAdded && options.FailOnNewPatterns && !allowed.Contains(change.Subject))
            {
                Add("RKP002", "Unexpected new pattern.", change.Subject);
            }

            if (change.Kind == ChangeKind.PatternRemoved && options.FailOnRemovedPatterns)
            {
                Add("RKP003", "Pattern was removed.", change.Subject);
            }

            if (change.Kind == ChangeKind.NamespaceAdded && options.FailOnNewNamespaces || change.Kind == ChangeKind.NamespaceRemoved && options.FailOnRemovedNamespaces)
            {
                Add("RKP004", "Unexpected namespace change.", change.Subject);
            }

            if (change.Kind == ChangeKind.FindingIntroduced && options.FailOnNewErrors && change.Severity == FindingSeverity.Error)
            {
                Add("RKP005", "New error finding.", change.Subject);
            }

            if (change.Kind == ChangeKind.PatternCountChanged && options.MaximumPatternIncreasePercent is decimal maximum &&
                long.TryParse(change.OldValue, out long oldValue) && long.TryParse(change.NewValue, out long newValue) && newValue > oldValue)
            {
                decimal percent = oldValue == 0 ? 100 : (newValue - oldValue) * 100m / oldValue;
                if (percent > maximum)
                {
                    Add("RKP008", $"Pattern growth {percent:F1}% exceeds {maximum:F1}%.", change.Subject);
                }
            }
        }
        if (current.MaximumDepth > options.MaximumAllowedDepth)
        {
            Add("RKP006", $"Maximum depth {current.MaximumDepth} exceeds {options.MaximumAllowedDepth}.");
        }

        if (current.NamespaceCount > options.MaximumAllowedNamespaces)
        {
            Add("RKP007", $"Namespace count {current.NamespaceCount} exceeds {options.MaximumAllowedNamespaces}.");
        }

        ImmutableArray<PolicyViolation> ordered = [.. violations.OrderBy(item => item.RuleId, StringComparer.Ordinal).ThenBy(item => item.Subject, StringComparer.Ordinal)];
        return new(ordered.IsEmpty, ordered, diff.CompatibilityErrors);

        void Add(string id, string message, string? subject = null) => violations.Add(new(id, message, subject));
    }
}

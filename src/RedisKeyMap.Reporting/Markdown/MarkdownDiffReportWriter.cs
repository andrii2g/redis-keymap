using System.Text;
using RedisKeyMap.Application.Abstractions;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Reporting.Markdown;

public sealed class MarkdownDiffReportWriter : IDiffReportWriter
{
    public Task WriteAsync(DiffResult result, PolicyEvaluation? policy, string path, CancellationToken cancellationToken) =>
        AtomicTextWriter.WriteAsync(path, Render(result, policy), cancellationToken);

    public string Render(DiffResult result, PolicyEvaluation? policy = null)
    {
        StringBuilder builder = new();
        builder.AppendLine("# Redis KeyMap Drift Report").AppendLine();
        builder.Append("Baseline: ").AppendLine(result.OldSnapshotDescription).Append("Current: ").AppendLine(result.NewSnapshotDescription).AppendLine();
        Section("Compatibility warnings", result.CompatibilityErrors);
        builder.AppendLine("## Summary by severity/change kind").AppendLine();
        builder.Append("- Info: ").AppendLine(result.Summary.Info.ToString()).Append("- Warnings: ").AppendLine(result.Summary.Warnings.ToString()).Append("- Errors: ").AppendLine(result.Summary.Errors.ToString()).AppendLine();
        Changes("Added patterns", result, ChangeKind.PatternAdded);
        Changes("Removed patterns", result, ChangeKind.PatternRemoved);
        Changes("Count changes", result, ChangeKind.PatternCountChanged);
        Changes("Namespace changes", result, ChangeKind.NamespaceAdded, ChangeKind.NamespaceRemoved, ChangeKind.NamespaceCountChanged);
        Changes("Finding changes", result, ChangeKind.FindingIntroduced, ChangeKind.FindingResolved);
        builder.AppendLine("## Policy result").AppendLine();
        if (policy is null)
        {
            builder.AppendLine("Not evaluated.");
        }
        else if (policy.Passed)
        {
            builder.AppendLine("Passed.");
        }
        else
        {
            foreach (PolicyViolation violation in policy.Violations)
            {
                builder.Append("- **").Append(violation.RuleId).Append("**: ").AppendLine(violation.Message);
            }
        }
        builder.AppendLine().AppendLine("## Limitations").AppendLine().AppendLine("Incomplete scans and changed normalization can make apparent drift inconclusive.");
        return builder.ToString();

        void Section(string title, IEnumerable<string> items)
        {
            builder.Append("## ").AppendLine(title).AppendLine();
            string[] array = items.ToArray();
            if (array.Length == 0)
            {
                builder.AppendLine("None.");
            }
            else
            {
                foreach (string item in array)
                {
                    builder.Append("- ").AppendLine(item);
                }
            }
            builder.AppendLine();
        }

        void Changes(string title, DiffResult value, params ChangeKind[] kinds)
        {
            builder.Append("## ").AppendLine(title).AppendLine();
            DiffChange[] changes = value.Changes.Where(change => kinds.Contains(change.Kind)).ToArray();
            if (changes.Length == 0)
            {
                builder.AppendLine("None.");
            }
            else
            {
                foreach (DiffChange change in changes)
                {
                    builder.Append("- ").Append(MarkdownEscaper.Code(change.Subject)).Append(": ").AppendLine(change.Message);
                }
            }
            builder.AppendLine();
        }
    }
}

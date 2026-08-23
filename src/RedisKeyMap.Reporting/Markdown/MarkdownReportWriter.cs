using System.Globalization;
using System.Text;
using RedisKeyMap.Application.Abstractions;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Reporting.Markdown;

public sealed class MarkdownReportWriter : IAnalysisReportWriter
{
    private readonly int _maximumPatternRows;
    private readonly bool _showTreeCounts;
    private readonly int _maximumTreeDepth;

    public MarkdownReportWriter(int maximumPatternRows = 1000, bool showTreeCounts = true, int maximumTreeDepth = 20)
    {
        _maximumPatternRows = maximumPatternRows;
        _showTreeCounts = showTreeCounts;
        _maximumTreeDepth = maximumTreeDepth;
    }

    public Task WriteAsync(Snapshot snapshot, string path, CancellationToken cancellationToken) =>
        AtomicTextWriter.WriteAsync(path, Render(snapshot), cancellationToken);

    public string Render(Snapshot snapshot)
    {
        StringBuilder builder = new();
        builder.AppendLine("# Redis KeyMap Report").AppendLine();
        builder.Append("Generated: ").Append(snapshot.GeneratedAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)).AppendLine("  ");
        builder.Append("Tool version: ").AppendLine(snapshot.ToolVersion).AppendLine();
        builder.AppendLine("> [!IMPORTANT]").AppendLine("> This report contains key-name structure only; Redis values were not read. Masked reports can still reveal architecture.").AppendLine();
        if (!snapshot.Scan.IsComplete)
        {
            builder.AppendLine("> [!WARNING]").AppendLine("> This snapshot is incomplete. Counts and removed-pattern conclusions may be inaccurate.").AppendLine();
        }
        if (snapshot.Source.ContainsRawExamples)
        {
            builder.AppendLine("> [!CAUTION]").AppendLine("> This report contains raw Redis key examples and may contain sensitive identifiers.").AppendLine();
        }

        builder.AppendLine("## Summary").AppendLine();
        builder.AppendLine("| Metric | Value |").AppendLine("|---|---:|");
        Row("Accepted unique keys", snapshot.TotalKeys.ToString("N0", CultureInfo.InvariantCulture));
        Row("Unique normalized patterns", snapshot.UniquePatterns.ToString("N0", CultureInfo.InvariantCulture));
        Row("Top-level namespaces", snapshot.NamespaceCount.ToString("N0", CultureInfo.InvariantCulture));
        Row("Maximum key depth", snapshot.MaximumDepth.ToString(CultureInfo.InvariantCulture));
        Row("Duplicate observations ignored", snapshot.Scan.DuplicateItemsIgnored.ToString("N0", CultureInfo.InvariantCulture));
        Row("Complete scan", snapshot.Scan.IsComplete ? "Yes" : "No");

        builder.AppendLine().AppendLine("## Scan metadata").AppendLine();
        builder.Append("- Observed items: ").Append(snapshot.Scan.ObservedItems.ToString("N0", CultureInfo.InvariantCulture)).AppendLine();
        builder.Append("- Duration: ").Append(snapshot.Scan.DurationMilliseconds.ToString("N0", CultureInfo.InvariantCulture)).AppendLine(" ms");
        builder.Append("- Duplicate handling: ").AppendLine(snapshot.Scan.DuplicateHandling.ToString()).AppendLine();

        builder.AppendLine("## Top-level namespaces").AppendLine();
        builder.AppendLine("| Namespace | Count |").AppendLine("|---|---:|");
        foreach (NamespaceStats item in snapshot.Namespaces)
        {
            builder.Append("| ").Append(MarkdownEscaper.TableCell(item.Name)).Append(" | ").Append(item.Count.ToString("N0", CultureInfo.InvariantCulture)).AppendLine(" |");
        }

        Tree("Technical key hierarchy", snapshot.TechnicalTree);
        Tree("Simplified logical hierarchy", snapshot.LogicalTree);

        builder.AppendLine("## Normalized patterns").AppendLine();
        builder.AppendLine("| Pattern | Count | Examples |").AppendLine("|---|---:|---|");
        foreach (PatternStats pattern in snapshot.Patterns.Take(_maximumPatternRows))
        {
            string examples = string.Join(", ", pattern.Examples.Select(value => MarkdownEscaper.Code(MarkdownEscaper.TableCell(value))));
            builder.Append("| ").Append(MarkdownEscaper.Code(MarkdownEscaper.TableCell(pattern.Pattern))).Append(" | ")
                .Append(pattern.Count.ToString("N0", CultureInfo.InvariantCulture)).Append(" | ").Append(examples).AppendLine(" |");
        }
        if (snapshot.Patterns.Length > _maximumPatternRows)
        {
            builder.AppendLine().Append(snapshot.Patterns.Length - _maximumPatternRows).AppendLine(" pattern rows omitted.");
        }

        builder.AppendLine().AppendLine("## Findings").AppendLine();
        if (snapshot.Findings.IsEmpty)
        {
            builder.AppendLine("No obvious structural issues detected.");
        }
        else
        {
            foreach (Finding finding in snapshot.Findings)
            {
                builder.Append("- **").Append(finding.RuleId).Append("** (").Append(finding.Severity).Append("): ").Append(finding.Message);
                if (finding.Pattern is not null)
                {
                    builder.Append(' ').Append(MarkdownEscaper.Code(finding.Pattern));
                }
                builder.AppendLine();
            }
        }

        builder.AppendLine().AppendLine("## Recommendations").AppendLine();
        foreach (string recommendation in snapshot.Recommendations)
        {
            builder.Append("- ").AppendLine(recommendation);
        }
        builder.AppendLine().AppendLine("## Method and limitations").AppendLine();
        builder.AppendLine("Redis KeyMap discovers naming hierarchy and structural patterns only. Live scans use cursor iteration; concurrent mutation can make counts approximate, and incomplete scans cannot prove removals.");
        return builder.ToString();

        void Row(string name, string value) => builder.Append("| ").Append(name).Append(" | ").Append(value).AppendLine(" |");

        void Tree(string title, IEnumerable<TreeSnapshotNode> nodes)
        {
            string fence = new((char)96, 3);
            builder.AppendLine().Append("## ").AppendLine(title).AppendLine().Append(fence).AppendLine("text");
            string rendered = new UnicodeTreeRenderer().Render(nodes, _showTreeCounts, _maximumTreeDepth);
            builder.AppendLine(rendered.Length == 0 ? "(empty)" : rendered).AppendLine(fence).AppendLine();
        }
    }
}

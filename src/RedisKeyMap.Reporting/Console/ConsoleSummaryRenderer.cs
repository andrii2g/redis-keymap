using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Reporting.Console;

public static class ConsoleSummaryRenderer
{
    public static string Render(Snapshot snapshot, string snapshotPath, string reportPath) =>
        $"Redis KeyMap {snapshot.ToolVersion}{Environment.NewLine}" +
        $"Scanned {snapshot.TotalKeys} unique keys in {snapshot.Scan.DurationMilliseconds / 1000d:F2} s{Environment.NewLine}" +
        $"Found {snapshot.NamespaceCount} namespaces and {snapshot.UniquePatterns} patterns{Environment.NewLine}" +
        $"Findings: {snapshot.Findings.Count(item => item.Severity == FindingSeverity.Error)} errors, {snapshot.Findings.Count(item => item.Severity == FindingSeverity.Warning)} warnings, {snapshot.Findings.Count(item => item.Severity == FindingSeverity.Info)} info{Environment.NewLine}" +
        $"Snapshot: {snapshotPath}{Environment.NewLine}Report:   {reportPath}";
}

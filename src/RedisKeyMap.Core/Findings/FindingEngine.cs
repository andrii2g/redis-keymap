using System.Collections.Immutable;
using RedisKeyMap.Core.Aggregation;
using RedisKeyMap.Core.Models;
using RedisKeyMap.Core.Utilities;

namespace RedisKeyMap.Core.Findings;

public static class FindingEngine
{
    private static readonly ImmutableSortedDictionary<string, string> EmptyEvidence = ImmutableSortedDictionary<string, string>.Empty;

    public static ImmutableArray<Finding> Evaluate(
        long totalKeys,
        int maximumDepth,
        IReadOnlyCollection<NamespaceStats> namespaces,
        IReadOnlyCollection<PatternStats> patterns,
        ImmutableArray<TreeSnapshotNode> tree,
        ScanMetadata scan,
        AnalysisOptions options)
    {
        List<Finding> findings = [];
        if (patterns.Any(pattern => pattern.HasEmptySegment))
        {
            Add("RKM001", FindingSeverity.Warning, "One or more keys contain an empty segment; review delimiter consistency.");
        }

        if (maximumDepth > options.MaximumDepthFinding)
        {
            Add("RKM002", FindingSeverity.Warning, $"Maximum key depth {maximumDepth} exceeds {options.MaximumDepthFinding}.");
        }

        if (namespaces.Count > options.MaximumNamespacesFinding)
        {
            Add("RKM003", FindingSeverity.Info, $"Keyspace contains {namespaces.Count} top-level namespaces.");
        }

        foreach (NamespaceStats item in namespaces.Where(item => item.Count == 1).Take(20))
        {
            Add("RKM004", FindingSeverity.Info, "Namespace contains exactly one accepted key.", item.Name);
        }

        foreach (PatternStats pattern in patterns.Where(pattern => pattern.HasHeuristicSegment).Take(20))
        {
            Add("RKM005", FindingSeverity.Info, "Pattern contains heuristic dynamic segments; confirm normalization.", pattern.Pattern);
        }

        foreach (string subject in FindCaseCollisions(tree).Take(20))
        {
            Add("RKM006", FindingSeverity.Warning, "Sibling segment names differ only by case.", subject);
        }

        if (patterns.Any(pattern => pattern.HasBinaryData))
        {
            Add("RKM007", FindingSeverity.Warning, "Binary keys were observed and cannot be structurally mapped.");
        }

        if (!scan.IsComplete)
        {
            Add("RKM008", FindingSeverity.Warning, "The scan is incomplete; treat this snapshot as partial.");
        }

        foreach (PatternStats pattern in patterns.Where(IsDirectEntityPattern).Take(20))
        {
            Add("RKM009", FindingSeverity.Info, "Namespace contains direct entity keys; document the stored base entity meaning.", pattern.Pattern);
        }

        if (patterns.Count > options.MaximumPatternsFinding)
        {
            Add("RKM010", FindingSeverity.Warning, $"Unique patterns exceed {options.MaximumPatternsFinding}.");
        }

        return StableOrdering.Findings(findings).ToImmutableArray();

        void Add(string id, FindingSeverity severity, string message, string? pattern = null) =>
            findings.Add(new(id, severity, message, pattern, EmptyEvidence));
    }

    private static bool IsDirectEntityPattern(PatternStats pattern) =>
        pattern.SegmentCount == 2 && (pattern.Pattern.EndsWith(":{id}", StringComparison.Ordinal) ||
            pattern.Pattern.EndsWith(":{uuid}", StringComparison.Ordinal) ||
            pattern.Pattern.EndsWith(":{ulid}", StringComparison.Ordinal));

    private static IEnumerable<string> FindCaseCollisions(IEnumerable<TreeSnapshotNode> nodes)
    {
        foreach (TreeSnapshotNode node in nodes)
        {
            foreach (IGrouping<string, TreeSnapshotNode> group in node.Children.GroupBy(child => child.Name, StringComparer.OrdinalIgnoreCase))
            {
                if (group.Select(child => child.Name).Distinct(StringComparer.Ordinal).Skip(1).Any())
                {
                    yield return $"{node.Name}/{group.Key}";
                }
            }
            foreach (string nested in FindCaseCollisions(node.Children))
            {
                yield return nested;
            }
        }
    }
}

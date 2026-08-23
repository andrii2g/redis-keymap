using System.Collections.Immutable;

namespace RedisKeyMap.Core.Models;

public sealed record Snapshot(
    int SchemaVersion,
    string ToolVersion,
    DateTimeOffset GeneratedAtUtc,
    string ConfigurationFingerprint,
    SnapshotSource Source,
    ScanMetadata Scan,
    long TotalKeys,
    int UniquePatterns,
    int NamespaceCount,
    int MaximumDepth,
    ImmutableArray<NamespaceStats> Namespaces,
    ImmutableArray<PatternStats> Patterns,
    ImmutableArray<TreeSnapshotNode> TechnicalTree,
    ImmutableArray<TreeSnapshotNode> LogicalTree,
    ImmutableArray<Finding> Findings,
    ImmutableArray<string> Recommendations);

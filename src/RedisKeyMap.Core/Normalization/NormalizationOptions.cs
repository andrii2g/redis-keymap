using System.Collections.Immutable;

namespace RedisKeyMap.Core.Normalization;

public sealed record LogicalTreeOptions
{
    public ImmutableArray<string> CollapseMarkers { get; init; } = SegmentMarkers.BuiltIns;
}

public sealed record NormalizationOptions
{
    public string Delimiter { get; init; } = ":";
    public ImmutableArray<CustomNormalizationRule> CustomRules { get; init; } = [];
    public LogicalTreeOptions LogicalTree { get; init; } = new();
}

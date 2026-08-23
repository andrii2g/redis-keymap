using System.Collections.Immutable;

namespace RedisKeyMap.Core.Policies;

public sealed record PolicyOptions
{
    public bool FailOnIncompleteScan { get; init; } = true;
    public bool FailOnNewPatterns { get; init; }
    public bool FailOnRemovedPatterns { get; init; }
    public bool FailOnNewNamespaces { get; init; }
    public bool FailOnRemovedNamespaces { get; init; }
    public bool FailOnNewErrors { get; init; } = true;
    public int MaximumAllowedDepth { get; init; } = 6;
    public int MaximumAllowedNamespaces { get; init; } = 30;
    public decimal? MaximumPatternIncreasePercent { get; init; }
    public ImmutableArray<string> AllowedPatterns { get; init; } = [];
    public ImmutableArray<string> IgnoredPatterns { get; init; } = [];
}

using System.Text.Json.Serialization;
using RedisKeyMap.Core.Models;
using RedisKeyMap.Core.Normalization;
using RedisKeyMap.Core.Policies;
using RedisKeyMap.Core.Privacy;

namespace RedisKeyMap.Application.Configuration;

public sealed record AnalysisConfiguration
{
    public DuplicateHandling DuplicateHandling { get; init; } = DuplicateHandling.Hash64;
    public int MaximumDepth { get; init; } = 6;
    public int MaximumNamespaces { get; init; } = 30;
    public int MaximumPatterns { get; init; } = 10_000;
}

public sealed record ReportConfiguration
{
    public bool ShowTreeCounts { get; init; } = true;
    public int MaximumTreeDepth { get; init; } = 20;
    public int MaximumPatternRows { get; init; } = 1000;
}

public sealed record AppConfiguration
{
    [JsonPropertyName("$schema")]
    public string? Schema { get; init; }
    public string Delimiter { get; init; } = ":";
    public NormalizationOptions Normalization { get; init; } = new();
    public AnalysisConfiguration Analysis { get; init; } = new();
    public PrivacyOptions Privacy { get; init; } = new();
    public ReportConfiguration Report { get; init; } = new();
    public PolicyOptions Policies { get; init; } = new();

    public AppConfiguration Effective() => this with { Normalization = Normalization with { Delimiter = Delimiter } };
}

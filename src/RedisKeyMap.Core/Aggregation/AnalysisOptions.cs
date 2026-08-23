using RedisKeyMap.Core.Models;
using RedisKeyMap.Core.Normalization;
using RedisKeyMap.Core.Privacy;

namespace RedisKeyMap.Core.Aggregation;

public sealed record AnalysisOptions
{
    public NormalizationOptions Normalization { get; init; } = new();
    public PrivacyOptions Privacy { get; init; } = new();
    public DuplicateHandling DuplicateHandling { get; init; } = DuplicateHandling.Hash64;
    public int MaximumDepthFinding { get; init; } = 6;
    public int MaximumNamespacesFinding { get; init; } = 30;
    public int MaximumPatternsFinding { get; init; } = 10_000;
    public long? MaxKeys { get; init; }
}

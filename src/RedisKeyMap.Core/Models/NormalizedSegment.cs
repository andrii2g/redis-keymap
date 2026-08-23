namespace RedisKeyMap.Core.Models;

public sealed record NormalizedSegment(string Original, string Value, SegmentKind Kind, NormalizationConfidence Confidence);

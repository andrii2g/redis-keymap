using System.Collections.Immutable;

namespace RedisKeyMap.Core.Models;

public sealed record PatternStats(string Pattern, long Count, int SegmentCount, ImmutableArray<string> Examples, bool HasEmptySegment, bool HasBinaryData, bool HasHeuristicSegment);

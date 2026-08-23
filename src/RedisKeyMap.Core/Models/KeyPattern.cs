using System.Collections.Immutable;

namespace RedisKeyMap.Core.Models;

public sealed record KeyPattern(
    string DisplayKey,
    ImmutableArray<NormalizedSegment> Segments,
    string Pattern,
    ImmutableArray<string> LogicalSegments,
    bool ContainsEmptySegment,
    bool ContainsBinaryData,
    bool ContainsHeuristicSegment);

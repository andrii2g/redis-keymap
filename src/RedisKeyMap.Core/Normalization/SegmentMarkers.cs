using System.Collections.Immutable;

namespace RedisKeyMap.Core.Normalization;

public static class SegmentMarkers
{
    public const string NumericId = "{id}";
    public const string Uuid = "{uuid}";
    public const string Ulid = "{ulid}";
    public const string Hex = "{hex}";
    public const string Token = "{token}";
    public const string Binary = "{binary}";

    public static ImmutableArray<string> BuiltIns { get; } = [NumericId, Uuid, Ulid, Hex, Token, Binary];
}

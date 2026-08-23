using System.Collections.Immutable;

namespace RedisKeyMap.Core.Models;

public sealed record TreeSnapshotNode(string Name, long Count, long TerminalCount, ImmutableArray<TreeSnapshotNode> Children);

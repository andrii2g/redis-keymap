namespace RedisKeyMap.Core.Models;

public sealed record SnapshotSource(SourceKind Kind, int? Database = null, string? Match = null, string? SourceLabel = null, bool ContainsRawExamples = false);

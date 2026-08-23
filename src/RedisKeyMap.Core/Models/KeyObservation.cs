namespace RedisKeyMap.Core.Models;

public sealed record KeyObservation(ReadOnlyMemory<byte> RawBytes, string? Text, string SourceEndpoint);

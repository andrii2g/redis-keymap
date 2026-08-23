using System.Collections.Immutable;

namespace RedisKeyMap.Core.Models;

public sealed record ScanMetadata(
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    long DurationMilliseconds,
    long ObservedItems,
    long AcceptedUniqueItems,
    long DuplicateItemsIgnored,
    long InvalidItemsIgnored,
    bool WasLimited,
    bool WasCancelled,
    bool IsComplete,
    DuplicateHandling DuplicateHandling,
    ImmutableArray<string> Endpoints,
    ImmutableArray<string> Warnings);

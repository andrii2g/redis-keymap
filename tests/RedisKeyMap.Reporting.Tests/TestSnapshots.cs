using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Reporting.Tests;

internal static class TestSnapshots
{
    public static Snapshot Create(bool complete = true, bool raw = false) => new(
        1,
        "1.0.0",
        new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero),
        new string('a', 64),
        new SnapshotSource(SourceKind.TextFile, SourceLabel: "sample", ContainsRawExamples: raw),
        new ScanMetadata(
            new DateTimeOffset(2026, 1, 2, 3, 4, 4, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero),
            1000, 2, 2, 0, 0, false, false, complete, DuplicateHandling.Hash64, ["file"], []),
        2, 1, 1, 2,
        [new("user", 2)],
        [new("user:{id}", 2, 2, ["user:{id}"], false, false, true)],
        [new("user", 2, 0, [new("{id}", 2, 2, [])])],
        [new("user", 2, 2, [])],
        [],
        ["Document patterns."]);
}

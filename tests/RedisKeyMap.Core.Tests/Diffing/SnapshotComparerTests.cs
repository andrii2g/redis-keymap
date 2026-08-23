using System.Collections.Immutable;
using RedisKeyMap.Core.Diffing;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Core.Tests.Diffing;

public sealed class SnapshotComparerTests
{
    [Fact]
    public void Compare_WhenPatternAddedAndRemoved_OrdersAndClassifiesChanges()
    {
        Snapshot oldSnapshot = Create([new("old", 1, 1, [], false, false, false)]);
        Snapshot newSnapshot = Create([new("new", 2, 1, [], false, false, false)]);
        DiffResult result = new SnapshotComparer().Compare(oldSnapshot, newSnapshot);
        Assert.True(result.IsCompatible);
        Assert.Contains(result.Changes, change => change.Kind == ChangeKind.PatternAdded && change.Subject == "new");
        Assert.Contains(result.Changes, change => change.Kind == ChangeKind.PatternRemoved && change.Subject == "old");
    }

    public static Snapshot Create(ImmutableArray<PatternStats> patterns, bool complete = true) => new(
        1, "test", DateTimeOffset.UnixEpoch, "same", new(SourceKind.TextFile),
        new(DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch, 0, patterns.Sum(item => item.Count), patterns.Sum(item => item.Count), 0, 0, false, false, complete, DuplicateHandling.Hash64, ["file"], []),
        patterns.Sum(item => item.Count), patterns.Length, 1, 1, [new("root", 1)], patterns, [], [], [], []);
}

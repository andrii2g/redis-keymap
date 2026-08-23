using RedisKeyMap.Core.Aggregation;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Core.Tests.Aggregation;

public sealed class KeyspaceAnalyzerTests
{
    [Fact]
    public async Task AnalyzeAsync_WhenDuplicateKeys_UsesUniqueCountsAndMaskedExamples()
    {
        AnalysisResult result = await new KeyspaceAnalyzer().AnalyzeAsync(
            TestData.Observations("user:123", "user:123", "user:456:orders"),
            new(),
            new SnapshotSource(SourceKind.TextFile),
            TimeProvider.System,
            "test",
            TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Snapshot.TotalKeys);
        Assert.Equal(1, result.Snapshot.Scan.DuplicateItemsIgnored);
        Assert.All(result.Snapshot.Patterns.SelectMany(pattern => pattern.Examples), example => Assert.DoesNotContain("123", example));
    }

    [Fact]
    public async Task AnalyzeAsync_WhenLogicalPathsCollide_AggregatesTreeCount()
    {
        AnalysisResult result = await new KeyspaceAnalyzer().AnalyzeAsync(
            TestData.Observations("user:1:orders", "user:2:orders"),
            new(),
            new SnapshotSource(SourceKind.TextFile),
            TimeProvider.System,
            "test",
            TestContext.Current.CancellationToken);

        var user = Assert.Single(result.Snapshot.LogicalTree);
        var orders = Assert.Single(user.Children);
        Assert.Equal(2, orders.Count);
    }
}

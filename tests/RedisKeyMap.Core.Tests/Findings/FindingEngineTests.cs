using RedisKeyMap.Core.Findings;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Core.Tests.Findings;

public sealed class FindingEngineTests
{
    [Fact]
    public void Evaluate_WhenEveryConditionPresent_ProducesRkm001ThroughRkm010()
    {
        NamespaceStats[] namespaces = [new("User", 1), new("user", 1)];
        PatternStats[] patterns =
        [
            new("user::value", 1, 3, [], true, false, false),
            new("user:{id}", 1, 2, [], false, false, true),
            new("{binary}", 1, 1, [], false, true, false)
        ];
        TreeSnapshotNode[] tree =
        [
            new("root", 2, 0, [new("User", 1, 1, []), new("user", 1, 1, [])])
        ];
        ScanMetadata scan = Scan(complete: false);
        var findings = FindingEngine.Evaluate(3, 7, namespaces, patterns, [.. tree], scan, new()
        {
            MaximumDepthFinding = 6,
            MaximumNamespacesFinding = 1,
            MaximumPatternsFinding = 1
        });

        for (int number = 1; number <= 10; number++)
        {
            Assert.Contains(findings, finding => finding.RuleId == $"RKM{number:000}");
        }
    }

    [Fact]
    public void Evaluate_WhenCleanAggregate_ReturnsNoFindings()
    {
        var findings = FindingEngine.Evaluate(
            2,
            1,
            [new("stable", 2)],
            [new("stable", 2, 1, [], false, false, false)],
            [new("stable", 2, 2, [])],
            Scan(complete: true),
            new());
        Assert.Empty(findings);
    }

    private static ScanMetadata Scan(bool complete) => new(
        DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch, 0, 2, 2, 0, 0, false, false, complete, DuplicateHandling.Hash64, ["file"], []);
}

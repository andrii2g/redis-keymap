using RedisKeyMap.Core.Models;
using RedisKeyMap.Core.Policies;
using RedisKeyMap.Core.Tests.Diffing;

namespace RedisKeyMap.Core.Tests.Policies;

public sealed class PolicyEvaluatorAllRulesTests
{
    [Fact]
    public void Evaluate_WhenAllConditionsPresent_ProducesRkp001ThroughRkp008()
    {
        DiffChange[] changes =
        [
            new(ChangeKind.PatternAdded, FindingSeverity.Info, "new", null, "1", "added"),
            new(ChangeKind.PatternRemoved, FindingSeverity.Warning, "removed", "1", null, "removed"),
            new(ChangeKind.NamespaceAdded, FindingSeverity.Info, "namespace", null, "1", "added"),
            new(ChangeKind.FindingIntroduced, FindingSeverity.Error, "RKM999", null, "error", "finding"),
            new(ChangeKind.PatternCountChanged, FindingSeverity.Info, "growing", "1", "3", "growth")
        ];
        DiffResult diff = new(true, [], "old", "new", [.. changes], new(2, 1, 1));
        Snapshot current = SnapshotComparerTests.Create([], false) with { MaximumDepth = 7, NamespaceCount = 31 };
        PolicyOptions options = new()
        {
            FailOnNewPatterns = true,
            FailOnRemovedPatterns = true,
            FailOnNewNamespaces = true,
            FailOnNewErrors = true,
            MaximumAllowedDepth = 6,
            MaximumAllowedNamespaces = 30,
            MaximumPatternIncreasePercent = 50
        };

        PolicyEvaluation result = new PolicyEvaluator().Evaluate(diff, current, options);
        for (int number = 1; number <= 8; number++)
        {
            Assert.Contains(result.Violations, violation => violation.RuleId == $"RKP{number:000}");
        }
    }

    [Fact]
    public void Evaluate_WhenAddedPatternAllowedOrIgnored_Passes()
    {
        DiffChange change = new(ChangeKind.PatternAdded, FindingSeverity.Info, "allowed", null, "1", "added");
        DiffResult diff = new(true, [], "old", "new", [change], new(1, 0, 0));
        Snapshot current = SnapshotComparerTests.Create([]);
        PolicyEvaluation allowed = new PolicyEvaluator().Evaluate(diff, current, new() { FailOnNewPatterns = true, AllowedPatterns = ["allowed"] });
        PolicyEvaluation ignored = new PolicyEvaluator().Evaluate(diff, current, new() { FailOnNewPatterns = true, IgnoredPatterns = ["allowed"] });
        Assert.True(allowed.Passed);
        Assert.True(ignored.Passed);
    }
}

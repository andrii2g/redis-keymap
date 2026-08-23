using RedisKeyMap.Core.Diffing;
using RedisKeyMap.Core.Models;
using RedisKeyMap.Core.Policies;
using RedisKeyMap.Core.Tests.Diffing;

namespace RedisKeyMap.Core.Tests.Policies;

public sealed class PolicyEvaluatorTests
{
    [Fact]
    public void Evaluate_WhenNewPatternPolicyEnabled_ReturnsRkp002()
    {
        Snapshot baseline = SnapshotComparerTests.Create([]);
        Snapshot current = SnapshotComparerTests.Create([new("new", 1, 1, [], false, false, false)]);
        DiffResult diff = new SnapshotComparer().Compare(baseline, current);
        PolicyEvaluation result = new PolicyEvaluator().Evaluate(diff, current, new() { FailOnNewPatterns = true });
        Assert.False(result.Passed);
        Assert.Contains(result.Violations, violation => violation.RuleId == "RKP002");
    }

    [Fact]
    public void Evaluate_WhenIncomplete_ReturnsRkp001()
    {
        Snapshot current = SnapshotComparerTests.Create([], false);
        DiffResult diff = new SnapshotComparer().Compare(current, current);
        PolicyEvaluation result = new PolicyEvaluator().Evaluate(diff, current, new());
        Assert.Contains(result.Violations, violation => violation.RuleId == "RKP001");
    }
}

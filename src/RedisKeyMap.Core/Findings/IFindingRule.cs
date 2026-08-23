using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Core.Findings;

public interface IFindingRule
{
    string RuleId { get; }
    IEnumerable<Finding> Evaluate(Snapshot snapshot);
}

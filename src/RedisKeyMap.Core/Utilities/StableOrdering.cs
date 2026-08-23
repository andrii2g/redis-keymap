using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Core.Utilities;

public static class StableOrdering
{
    public static IOrderedEnumerable<PatternStats> Patterns(IEnumerable<PatternStats> values) =>
        values.OrderByDescending(value => value.Count).ThenBy(value => value.Pattern, StringComparer.Ordinal);

    public static IOrderedEnumerable<NamespaceStats> Namespaces(IEnumerable<NamespaceStats> values) =>
        values.OrderByDescending(value => value.Count).ThenBy(value => value.Name, StringComparer.Ordinal);

    public static IOrderedEnumerable<Finding> Findings(IEnumerable<Finding> values) =>
        values.OrderByDescending(value => value.Severity).ThenBy(value => value.RuleId, StringComparer.Ordinal).ThenBy(value => value.Pattern, StringComparer.Ordinal);
}

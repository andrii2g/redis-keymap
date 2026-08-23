using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Core.Aggregation;

internal sealed class PatternAccumulator(KeyPattern pattern)
{
    private readonly List<string> _examples = [];
    private readonly HashSet<string> _exampleSet = new(StringComparer.Ordinal);

    public KeyPattern Pattern { get; } = pattern;
    public long Count { get; private set; }

    public void Add(string? example, int limit)
    {
        Count++;
        if (example is not null && _examples.Count < limit && _exampleSet.Add(example))
        {
            _examples.Add(example);
        }
    }

    public PatternStats ToStats() => new(
        Pattern.Pattern,
        Count,
        Pattern.Segments.Length,
        [.. _examples.Order(StringComparer.Ordinal)],
        Pattern.ContainsEmptySegment,
        Pattern.ContainsBinaryData,
        Pattern.ContainsHeuristicSegment);
}

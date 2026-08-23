namespace RedisKeyMap.Core.Aggregation;

internal sealed class TrieNode(string name)
{
    public string Name { get; } = name;
    public long ThroughCount { get; set; }
    public long TerminalCount { get; set; }
    public Dictionary<string, TrieNode> Children { get; } = new(StringComparer.Ordinal);
}

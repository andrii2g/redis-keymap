using System.Collections.Immutable;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Core.Aggregation;

public sealed class PatternTrie
{
    private readonly TrieNode _root = new(string.Empty);

    public void Insert(IEnumerable<string> segments)
    {
        TrieNode current = _root;
        bool any = false;
        foreach (string segment in segments)
        {
            any = true;
            string name = segment.Length == 0 ? "(empty)" : segment;
            if (!current.Children.TryGetValue(name, out TrieNode? child))
            {
                child = new(name);
                current.Children.Add(name, child);
            }
            child.ThroughCount++;
            current = child;
        }
        if (any)
        {
            current.TerminalCount++;
        }
    }

    public ImmutableArray<TreeSnapshotNode> ToSnapshot() =>
        _root.Children.Values.OrderBy(node => node.Name, StringComparer.Ordinal).Select(Convert).ToImmutableArray();

    private static TreeSnapshotNode Convert(TrieNode node) => new(
        node.Name,
        node.ThroughCount,
        node.TerminalCount,
        node.Children.Values.OrderBy(child => child.Name, StringComparer.Ordinal).Select(Convert).ToImmutableArray());
}

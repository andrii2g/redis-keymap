using RedisKeyMap.Core.Aggregation;

namespace RedisKeyMap.Core.Tests.Aggregation;

public sealed class PatternTrieTests
{
    [Fact]
    public void ToSnapshot_WhenInsertedOutOfOrder_UsesOrdinalOrdering()
    {
        PatternTrie trie = new();
        trie.Insert(["z"]);
        trie.Insert(["a"]);
        Assert.Equal(["a", "z"], trie.ToSnapshot().Select(node => node.Name));
    }

    [Fact]
    public void ToSnapshot_WhenNodeTerminalAndParent_PreservesBothCounts()
    {
        PatternTrie trie = new();
        trie.Insert(["user"]);
        trie.Insert(["user", "{id}"]);
        var user = Assert.Single(trie.ToSnapshot());
        Assert.Equal(2, user.Count);
        Assert.Equal(1, user.TerminalCount);
    }
}

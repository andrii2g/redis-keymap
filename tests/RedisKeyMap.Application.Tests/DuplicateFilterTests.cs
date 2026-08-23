using System.Text;
using RedisKeyMap.Application.Sources;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Application.Tests;

public sealed class DuplicateFilterTests
{
    [Theory]
    [InlineData(DuplicateHandling.Hash64)]
    [InlineData(DuplicateHandling.Exact)]
    public void Accept_WhenSameBytesObservedTwice_IgnoresSecond(DuplicateHandling mode)
    {
        DuplicateFilter filter = new(mode);
        Assert.True(filter.Accept(Encoding.UTF8.GetBytes("same")));
        Assert.False(filter.Accept(Encoding.UTF8.GetBytes("same")));
        Assert.Equal(2, filter.ObservedItems);
        Assert.Equal(1, filter.DuplicateItems);
    }

    [Fact]
    public void Accept_WhenModeNone_AcceptsRepeats()
    {
        DuplicateFilter filter = new(DuplicateHandling.None);
        Assert.True(filter.Accept(new byte[] { 1 }));
        Assert.True(filter.Accept(new byte[] { 1 }));
    }
}

using StackExchange.Redis;

namespace RedisKeyMap.Redis.Tests;

public sealed class RedisResultParserTests
{
    [Fact]
    public void Parse_WhenValidPage_PreservesCursorAndRawBytes()
    {
        RedisResult result = RedisResult.Create(
        [
            RedisResult.Create((RedisValue)"42"),
            RedisResult.Create([(RedisValue)new byte[] { 0, 255 }])
        ]);
        RedisScanPage page = RedisResultParser.Parse(result);
        Assert.Equal(42UL, page.Cursor);
        Assert.Equal(new byte[] { 0, 255 }, Assert.Single(page.Keys));
    }

    [Fact]
    public void Parse_WhenWrongShape_Throws()
    {
        RedisResult result = RedisResult.Create([(RedisValue)"0"]);
        Assert.Throws<InvalidDataException>(() => RedisResultParser.Parse(result));
    }
}

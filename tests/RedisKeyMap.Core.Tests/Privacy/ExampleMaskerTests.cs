using RedisKeyMap.Core.Normalization;
using RedisKeyMap.Core.Privacy;

namespace RedisKeyMap.Core.Tests.Privacy;

public sealed class ExampleMaskerTests
{
    [Fact]
    public void Mask_WhenDefaultMode_DoesNotRetainDynamicIdentifier()
    {
        var observation = TestData.Observation("user:123");
        var pattern = new KeyNormalizer(new()).Normalize(observation);
        string? example = new ExampleMasker(new(), ":").Mask(observation, pattern);
        Assert.Equal("user:{id}", example);
        Assert.DoesNotContain("123", example);
    }
}

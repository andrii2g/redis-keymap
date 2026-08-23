using RedisKeyMap.Core.Normalization;

namespace RedisKeyMap.Core.Tests.Normalization;

public sealed class NormalizationRuleCompilerTests
{
    [Fact]
    public void Compile_WhenNamesDuplicate_Throws()
    {
        Assert.Throws<ArgumentException>(() => NormalizationRuleCompiler.Compile(
            [new("same", "^a$", "{a}"), new("same", "^b$", "{b}")]));
    }

    [Fact]
    public void Compile_WhenReplacementInvalid_Throws()
    {
        Assert.Throws<ArgumentException>(() => NormalizationRuleCompiler.Compile([new("bad", "^a$", "raw")]));
    }
}

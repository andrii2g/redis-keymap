using RedisKeyMap.Cli.Infrastructure;

namespace RedisKeyMap.Cli.Tests;

public sealed class SecretRedactionTests
{
    [Fact]
    public void Redact_WhenNestedMessageContainsSecrets_RemovesEverySecretForm()
    {
        string result = SecretRedactor.Redact("redis://user:secret@host,password=other,pwd=third nested exact-token", "exact-token");
        Assert.DoesNotContain("secret", result);
        Assert.DoesNotContain("other", result);
        Assert.DoesNotContain("third", result);
        Assert.DoesNotContain("exact-token", result);
    }
}

namespace RedisKeyMap.Redis.Tests;

public sealed class ConnectionStringSanitizerTests
{
    [Theory]
    [InlineData("redis://user:secret@example:6379", "secret")]
    [InlineData("host:6379,password=secret", "secret")]
    [InlineData("host:6379,PWD=secret", "secret")]
    public void Sanitize_WhenPasswordPresent_RemovesSecret(string input, string secret)
    {
        string result = ConnectionStringSanitizer.Sanitize(input);
        Assert.DoesNotContain(secret, result);
        Assert.Contains("[REDACTED]", result);
    }
}

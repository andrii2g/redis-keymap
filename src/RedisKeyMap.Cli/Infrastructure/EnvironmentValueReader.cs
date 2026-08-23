namespace RedisKeyMap.Cli.Infrastructure;

public static class EnvironmentValueReader
{
    public static string ReadRequired(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"Environment variable '{name}' is not set or is empty.") : value;
    }
}

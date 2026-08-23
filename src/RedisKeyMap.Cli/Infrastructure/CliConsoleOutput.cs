namespace RedisKeyMap.Cli.Infrastructure;

public static class CliConsoleOutput
{
    public static void Error(string message) => Console.Error.WriteLine($"Error: {message}");
    public static void Warning(string message) => Console.Error.WriteLine($"Warning: {message}");
}

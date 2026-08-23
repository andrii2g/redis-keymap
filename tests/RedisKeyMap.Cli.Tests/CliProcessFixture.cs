using System.Diagnostics;

namespace RedisKeyMap.Cli.Tests;

internal static class CliProcessFixture
{
    public static string RepositoryRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "RedisKeyMap.slnx")))
        {
            current = current.Parent;
        }
        return current?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }

    public static async Task<(int ExitCode, string Output, string Error)> RunAsync(params string[] arguments)
    {
        string root = RepositoryRoot();
        string dll = Path.Combine(root, "src", "RedisKeyMap.Cli", "bin", "Release", "net10.0", "redis-keymap.dll");
        ProcessStartInfo start = new("dotnet")
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        start.ArgumentList.Add(dll);
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(start) ?? throw new InvalidOperationException("CLI process failed to start.");
        string output = await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        string error = await process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        return (process.ExitCode, output, error);
    }
}

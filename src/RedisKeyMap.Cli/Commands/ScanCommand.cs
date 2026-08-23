using System.CommandLine;
using RedisKeyMap.Cli.Infrastructure;

namespace RedisKeyMap.Cli.Commands;

public static class ScanCommand
{
    public static Command Create()
    {
        Command command = new("scan", "Scan a standalone Redis database using explicit cursor iteration.");
        command.SetAction(_ =>
        {
            CliConsoleOutput.Error("Redis scan support is not yet available in this build.");
            return ExitCodes.OperationalFailure;
        });
        return command;
    }
}

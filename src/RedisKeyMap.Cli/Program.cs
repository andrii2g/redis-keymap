using System.CommandLine;
using System.CommandLine.Parsing;
using RedisKeyMap.Cli.Commands;
using RedisKeyMap.Cli.Infrastructure;

RootCommand root = RootCommandFactory.Create();
ParseResult parseResult = root.Parse(args);
if (parseResult.Errors.Count > 0)
{
    foreach (ParseError error in parseResult.Errors)
    {
        CliConsoleOutput.Error(error.Message);
    }
    return ExitCodes.UsageError;
}

return await parseResult.InvokeAsync(new InvocationConfiguration());

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Globalization;
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

InvocationConfiguration invocation = new();
StringWriter? redirectedOutput = null;
if (Console.IsOutputRedirected)
{
    redirectedOutput = new(CultureInfo.InvariantCulture);
    invocation.Output = redirectedOutput;
}
int exitCode = await parseResult.InvokeAsync(invocation);
if (redirectedOutput is not null)
{
    Console.Out.Write(TerminalCapabilities.StripAnsi(redirectedOutput.ToString()));
}
return exitCode;

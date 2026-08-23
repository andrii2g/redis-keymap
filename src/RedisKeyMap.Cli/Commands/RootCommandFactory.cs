using System.CommandLine;
using System.Reflection;
using RedisKeyMap.Application.Configuration;
using RedisKeyMap.Application.Requests;
using RedisKeyMap.Application.Results;
using RedisKeyMap.Cli.Infrastructure;
using RedisKeyMap.Core.Models;
using RedisKeyMap.Core.Privacy;
using RedisKeyMap.Reporting.Console;

namespace RedisKeyMap.Cli.Commands;

public static class RootCommandFactory
{
    public static RootCommand Create()
    {
        RootCommand root = new("Map a Redis keyspace, create sanitized snapshots, and detect schema drift.");
        Option<string?> configOption = new("--config") { Recursive = true };
        Option<bool> quietOption = new("--quiet") { Recursive = true };
        Option<bool> verboseOption = new("--verbose") { Recursive = true };
        Option<bool> noColorOption = new("--no-color") { Recursive = true };
        root.Options.Add(configOption);
        root.Options.Add(quietOption);
        root.Options.Add(verboseOption);
        root.Options.Add(noColorOption);
        root.Subcommands.Add(CreateAnalyze(configOption, quietOption, verboseOption));
        root.Subcommands.Add(CreateRender(configOption, quietOption, verboseOption));
        root.Subcommands.Add(CreateDiff(configOption, quietOption, verboseOption));
        root.Subcommands.Add(CreateCheck(configOption, quietOption, verboseOption));
        root.Subcommands.Add(ScanCommand.Create());
        root.SetAction(parseResult =>
        {
            Console.WriteLine($"Redis KeyMap {Version}");
            return ExitCodes.Success;
        });
        return root;
    }

    public static string Version =>
        typeof(RootCommandFactory).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "0.1.0";

    private static Command CreateAnalyze(Option<string?> configOption, Option<bool> quietOption, Option<bool> verboseOption)
    {
        Command command = new("analyze", "Analyze one UTF-8 key per line from a text file.");
        Option<string> input = new("--input") { Required = true };
        Option<string?> delimiter = new("--delimiter");
        Option<long?> maxKeys = new("--max-keys");
        Option<string?> duplicate = new("--duplicate-handling");
        Option<string?> snapshot = new("--snapshot");
        Option<string?> report = new("--report");
        Option<string?> sourceLabel = new("--source-label");
        Option<int?> examples = new("--examples");
        Option<bool> noExamples = new("--no-examples");
        Option<bool> rawExamples = new("--include-raw-examples");
        Option<bool> preserveWhitespace = new("--preserve-whitespace");
        Option<bool> includeEmptyLines = new("--include-empty-lines");
        Add(command, input, delimiter, maxKeys, duplicate, snapshot, report, sourceLabel, examples, noExamples, rawExamples, preserveWhitespace, includeEmptyLines);
        command.SetAction(async (parse, cancellationToken) =>
        {
            try
            {
                if (!ValidateGlobals(parse, quietOption, verboseOption))
                {
                    return ExitCodes.UsageError;
                }

                if (parse.GetValue(noExamples) && parse.GetValue(rawExamples))
                {
                    CliConsoleOutput.Error("--include-raw-examples and --no-examples cannot be used together.");
                    return ExitCodes.UsageError;
                }
                if (parse.GetValue(maxKeys) <= 0)
                {
                    CliConsoleOutput.Error("--max-keys must be greater than zero.");
                    return ExitCodes.UsageError;
                }
                AppConfiguration configuration = await new ConfigurationLoader().LoadAsync(parse.GetValue(configOption), cancellationToken).ConfigureAwait(false);
                string? delimiterValue = parse.GetValue(delimiter);
                if (delimiterValue is not null)
                {
                    if (delimiterValue.Length == 0)
                    {
                        CliConsoleOutput.Error("--delimiter cannot be empty.");
                        return ExitCodes.UsageError;
                    }
                    configuration = configuration with { Delimiter = delimiterValue, Normalization = configuration.Normalization with { Delimiter = delimiterValue } };
                }
                if (parse.GetValue(duplicate) is string duplicateValue)
                {
                    if (!Enum.TryParse(duplicateValue, true, out DuplicateHandling mode))
                    {
                        CliConsoleOutput.Error("--duplicate-handling must be none, hash64, or exact.");
                        return ExitCodes.UsageError;
                    }
                    configuration = configuration with { Analysis = configuration.Analysis with { DuplicateHandling = mode } };
                }
                if (parse.GetValue(examples) is int count)
                {
                    configuration = configuration with { Privacy = configuration.Privacy with { ExamplesPerPattern = count } };
                }
                if (parse.GetValue(noExamples))
                {
                    configuration = configuration with { Privacy = configuration.Privacy with { ExampleMode = ExampleMode.None } };
                }
                if (parse.GetValue(rawExamples))
                {
                    CliConsoleOutput.Warning("raw Redis key examples may contain sensitive identifiers.");
                    configuration = configuration with { Privacy = configuration.Privacy with { ExampleMode = ExampleMode.Raw } };
                }

                string snapshotPath = parse.GetValue(snapshot) ?? "redis-keymap.snapshot.json";
                string reportPath = parse.GetValue(report) ?? "redis-keymap.report.md";
                AnalyzeRequest request = new(
                    parse.GetRequiredValue(input),
                    snapshotPath,
                    reportPath,
                    parse.GetValue(sourceLabel),
                    parse.GetValue(maxKeys),
                    parse.GetValue(preserveWhitespace),
                    parse.GetValue(includeEmptyLines));
                OperationResult result = await CompositionRoot.Analyze(configuration, Version).ExecuteAsync(request, configuration, cancellationToken).ConfigureAwait(false);
                if (!parse.GetValue(quietOption) && result.Snapshot is not null)
                {
                    Console.WriteLine(ConsoleSummaryRenderer.Render(result.Snapshot, snapshotPath, reportPath));
                }
                return ExitCodes.Success;
            }
            catch (OperationCanceledException)
            {
                return ExitCodes.Cancelled;
            }
            catch (ArgumentException exception)
            {
                CliConsoleOutput.Error(SecretRedactor.Redact(exception.Message));
                return ExitCodes.UsageError;
            }
            catch (Exception exception)
            {
                CliConsoleOutput.Error(SecretRedactor.Redact(exception.Message));
                return ExitCodes.OperationalFailure;
            }
        });
        return command;
    }

    private static Command CreateRender(Option<string?> configOption, Option<bool> quietOption, Option<bool> verboseOption)
    {
        Command command = new("render", "Render Markdown from a snapshot.");
        Argument<string> snapshot = new("snapshot");
        Option<string?> report = new("--report");
        command.Arguments.Add(snapshot);
        command.Options.Add(report);
        command.SetAction(async (parse, cancellationToken) =>
        {
            try
            {
                if (!ValidateGlobals(parse, quietOption, verboseOption))
                {
                    return ExitCodes.UsageError;
                }

                AppConfiguration configuration = await new ConfigurationLoader().LoadAsync(parse.GetValue(configOption), cancellationToken).ConfigureAwait(false);
                string reportPath = parse.GetValue(report) ?? "redis-keymap.report.md";
                await CompositionRoot.Render(configuration).ExecuteAsync(new(parse.GetRequiredValue(snapshot), reportPath), cancellationToken).ConfigureAwait(false);
                if (!parse.GetValue(quietOption))
                {
                    Console.WriteLine($"Report: {reportPath}");
                }

                return ExitCodes.Success;
            }
            catch (Exception exception)
            {
                CliConsoleOutput.Error(SecretRedactor.Redact(exception.Message));
                return exception is ArgumentException ? ExitCodes.UsageError : ExitCodes.OperationalFailure;
            }
        });
        return command;
    }

    private static Command CreateDiff(Option<string?> configOption, Option<bool> quietOption, Option<bool> verboseOption)
    {
        Command command = new("diff", "Compare two compatible snapshots.");
        Argument<string> oldSnapshot = new("old-snapshot");
        Argument<string> newSnapshot = new("new-snapshot");
        Option<string?> report = new("--report");
        Option<bool> allowConfig = new("--allow-config-mismatch");
        Option<bool> allowSource = new("--allow-source-mismatch");
        Option<decimal?> threshold = new("--count-change-threshold");
        command.Arguments.Add(oldSnapshot);
        command.Arguments.Add(newSnapshot);
        Add(command, report, allowConfig, allowSource, threshold);
        command.SetAction(async (parse, cancellationToken) =>
        {
            try
            {
                if (!ValidateGlobals(parse, quietOption, verboseOption))
                {
                    return ExitCodes.UsageError;
                }

                _ = await new ConfigurationLoader().LoadAsync(parse.GetValue(configOption), cancellationToken).ConfigureAwait(false);
                string reportPath = parse.GetValue(report) ?? "redis-keymap.diff.md";
                OperationResult result = await CompositionRoot.Diff().ExecuteAsync(new(
                    parse.GetRequiredValue(oldSnapshot), parse.GetRequiredValue(newSnapshot), reportPath,
                    parse.GetValue(allowConfig), parse.GetValue(allowSource), parse.GetValue(threshold) ?? 0), cancellationToken).ConfigureAwait(false);
                if (result.Status != OperationStatus.Success)
                {
                    CliConsoleOutput.Error(result.Message ?? "Snapshot comparison failed.");
                    return ExitCodes.OperationalFailure;
                }
                if (!parse.GetValue(quietOption))
                {
                    Console.WriteLine($"Drift changes: {result.Diff?.Changes.Length ?? 0}{Environment.NewLine}Report: {reportPath}");
                }

                return ExitCodes.Success;
            }
            catch (Exception exception)
            {
                CliConsoleOutput.Error(SecretRedactor.Redact(exception.Message));
                return exception is ArgumentException ? ExitCodes.UsageError : ExitCodes.OperationalFailure;
            }
        });
        return command;
    }

    private static Command CreateCheck(Option<string?> configOption, Option<bool> quietOption, Option<bool> verboseOption)
    {
        Command command = new("check", "Compare snapshots and enforce configured policies.");
        Argument<string> baseline = new("baseline");
        Argument<string> current = new("current");
        Option<string?> report = new("--report");
        Option<bool> allowConfig = new("--allow-config-mismatch");
        Option<bool> allowSource = new("--allow-source-mismatch");
        command.Arguments.Add(baseline);
        command.Arguments.Add(current);
        Add(command, report, allowConfig, allowSource);
        command.SetAction(async (parse, cancellationToken) =>
        {
            try
            {
                if (!ValidateGlobals(parse, quietOption, verboseOption))
                {
                    return ExitCodes.UsageError;
                }

                AppConfiguration configuration = await new ConfigurationLoader().LoadAsync(parse.GetValue(configOption), cancellationToken).ConfigureAwait(false);
                OperationResult result = await CompositionRoot.Check().ExecuteAsync(new(
                    parse.GetRequiredValue(baseline), parse.GetRequiredValue(current), parse.GetValue(report),
                    parse.GetValue(allowConfig), parse.GetValue(allowSource)), configuration, cancellationToken).ConfigureAwait(false);
                if (result.Status == OperationStatus.OperationalFailure)
                {
                    CliConsoleOutput.Error(result.Message ?? "Snapshot comparison failed.");
                    return ExitCodes.OperationalFailure;
                }
                if (result.Policy is not null && !parse.GetValue(quietOption))
                {
                    Console.WriteLine(result.Policy.Passed ? "Policy: passed" : $"Policy: failed ({result.Policy.Violations.Length} violations)");
                }
                return result.Status == OperationStatus.PolicyViolation ? ExitCodes.PolicyViolation : ExitCodes.Success;
            }
            catch (Exception exception)
            {
                CliConsoleOutput.Error(SecretRedactor.Redact(exception.Message));
                return exception is ArgumentException ? ExitCodes.UsageError : ExitCodes.OperationalFailure;
            }
        });
        return command;
    }

    private static bool ValidateGlobals(ParseResult parse, Option<bool> quiet, Option<bool> verbose)
    {
        if (parse.GetValue(quiet) && parse.GetValue(verbose))
        {
            CliConsoleOutput.Error("--quiet and --verbose cannot be used together.");
            return false;
        }
        return true;
    }

    private static void Add(Command command, params Option[] options)
    {
        foreach (Option option in options)
        {
            command.Options.Add(option);
        }
    }
}

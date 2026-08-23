using System.CommandLine;
using System.Globalization;
using RedisKeyMap.Application.Configuration;
using RedisKeyMap.Application.Requests;
using RedisKeyMap.Cli.Infrastructure;
using RedisKeyMap.Core.Models;
using RedisKeyMap.Core.Privacy;
using RedisKeyMap.Redis;
using RedisKeyMap.Reporting.Console;

namespace RedisKeyMap.Cli.Commands;

public static class ScanCommand
{
    public static Command Create(Option<string?> configOption, Option<bool> quietOption, Option<bool> verboseOption)
    {
        Command command = new("scan", "Scan a standalone Redis database using explicit cursor iteration.");
        Option<string?> connection = new("--connection");
        Option<string?> connectionEnvironment = new("--connection-env");
        Option<int?> database = new("--database");
        Option<string?> match = new("--match");
        Option<int?> pageSize = new("--page-size");
        Option<long?> maxKeys = new("--max-keys");
        Option<string?> delimiter = new("--delimiter");
        Option<string?> duplicate = new("--duplicate-handling");
        Option<string?> snapshot = new("--snapshot");
        Option<string?> report = new("--report");
        Option<string?> sourceLabel = new("--source-label");
        Option<int?> examples = new("--examples");
        Option<bool> noExamples = new("--no-examples");
        Option<bool> rawExamples = new("--include-raw-examples");
        Option<bool> hideEndpoints = new("--hide-endpoints");
        Option<string?> timeout = new("--timeout");
        foreach (Option option in new Option[] { connection, connectionEnvironment, database, match, pageSize, maxKeys, delimiter, duplicate, snapshot, report, sourceLabel, examples, noExamples, rawExamples, hideEndpoints, timeout })
        {
            command.Options.Add(option);
        }

        command.SetAction(async (parse, cancellationToken) =>
        {
            string? exactSecret = null;
            try
            {
                if (parse.GetValue(quietOption) && parse.GetValue(verboseOption))
                {
                    CliConsoleOutput.Error("--quiet and --verbose cannot be used together.");
                    return ExitCodes.UsageError;
                }
                string? direct = parse.GetValue(connection);
                string? environmentName = parse.GetValue(connectionEnvironment);
                if ((direct is null) == (environmentName is null))
                {
                    CliConsoleOutput.Error("provide exactly one Redis connection source: --connection or --connection-env.");
                    return ExitCodes.UsageError;
                }
                exactSecret = direct ?? EnvironmentValueReader.ReadRequired(environmentName!);
                int effectivePageSize = parse.GetValue(pageSize) ?? 1000;
                if (effectivePageSize <= 0)
                {
                    CliConsoleOutput.Error("--page-size must be greater than zero.");
                    return ExitCodes.UsageError;
                }
                if (parse.GetValue(maxKeys) <= 0)
                {
                    CliConsoleOutput.Error("--max-keys must be greater than zero.");
                    return ExitCodes.UsageError;
                }
                if (parse.GetValue(noExamples) && parse.GetValue(rawExamples))
                {
                    CliConsoleOutput.Error("--include-raw-examples and --no-examples cannot be used together.");
                    return ExitCodes.UsageError;
                }

                AppConfiguration configuration = await new ConfigurationLoader().LoadAsync(parse.GetValue(configOption), cancellationToken).ConfigureAwait(false);
                if (parse.GetValue(delimiter) is string delimiterValue)
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
                configuration = configuration with
                {
                    Privacy = configuration.Privacy with
                    {
                        ExampleMode = parse.GetValue(noExamples) ? ExampleMode.None : parse.GetValue(rawExamples) ? ExampleMode.Raw : configuration.Privacy.ExampleMode,
                        HideEndpoints = parse.GetValue(hideEndpoints) || configuration.Privacy.HideEndpoints
                    }
                };
                if (parse.GetValue(rawExamples))
                {
                    CliConsoleOutput.Warning("raw Redis key examples may contain sensitive identifiers.");
                }

                using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutSource.CancelAfter(ParseDuration(parse.GetValue(timeout) ?? "10m"));
                int effectiveDatabase = parse.GetValue(database) ?? 0;
                string effectiveMatch = parse.GetValue(match) ?? "*";
                string snapshotPath = parse.GetValue(snapshot) ?? "redis-keymap.snapshot.json";
                string reportPath = parse.GetValue(report) ?? "redis-keymap.report.md";
                await using RedisKeySource source = await RedisKeySource.ConnectAsync(exactSecret, new(effectiveDatabase, effectiveMatch, effectivePageSize), timeoutSource.Token).ConfigureAwait(false);
                ScanRequest request = new(effectiveDatabase, effectiveMatch, effectivePageSize, snapshotPath, reportPath, parse.GetValue(maxKeys), parse.GetValue(sourceLabel));
                var result = await CompositionRoot.Scan(configuration, RootCommandFactory.Version).ExecuteAsync(source, request, configuration, timeoutSource.Token).ConfigureAwait(false);
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
                CliConsoleOutput.Error(SecretRedactor.Redact(exception.Message, exactSecret));
                return ExitCodes.UsageError;
            }
            catch (Exception exception)
            {
                CliConsoleOutput.Error(SecretRedactor.Redact(exception.Message, exactSecret));
                return ExitCodes.OperationalFailure;
            }
        });
        return command;
    }

    private static TimeSpan ParseDuration(string value)
    {
        if (value.Length < 2 || !double.TryParse(value[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out double amount) || amount <= 0)
        {
            throw new ArgumentException("--timeout must be a positive duration such as 30s, 5m, or 1h.");
        }
        return value[^1] switch
        {
            's' => TimeSpan.FromSeconds(amount),
            'm' => TimeSpan.FromMinutes(amount),
            'h' => TimeSpan.FromHours(amount),
            _ => throw new ArgumentException("--timeout must use s, m, or h.")
        };
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;
using RedisKeyMap.Core.Models;
using RedisKeyMap.Core.Privacy;

namespace RedisKeyMap.Application.Configuration;

public sealed class ConfigurationLoader
{
    public async Task<AppConfiguration> LoadAsync(string? path, CancellationToken cancellationToken)
    {
        if (path is null)
        {
            return new AppConfiguration().Effective();
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Configuration file not found: {path}", path);
        }

        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        };
        options.Converters.Add(new JsonStringEnumConverter<DuplicateHandling>(JsonNamingPolicy.CamelCase));
        options.Converters.Add(new JsonStringEnumConverter<ExampleMode>(JsonNamingPolicy.CamelCase));
        AppConfigurationJsonContext context = new(options);
        await using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 16 * 1024, FileOptions.Asynchronous);
        AppConfiguration configuration;
        try
        {
            configuration = await JsonSerializer.DeserializeAsync(stream, context.AppConfiguration, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidDataException("Configuration file is empty.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"Invalid configuration JSON at {exception.Path ?? "$"}.", exception);
        }
        configuration = configuration.Effective();
        var errors = ConfigurationValidator.Validate(configuration);
        if (!errors.IsEmpty)
        {
            throw new ArgumentException(string.Join(Environment.NewLine, errors.Select(error => $"{error.Path}: {error.Message}")));
        }

        return configuration;
    }
}

using System.Text.Json;
using RedisKeyMap.Application.Abstractions;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Reporting.Json;

public sealed class SnapshotJsonReader : ISnapshotReader
{
    public async Task<Snapshot> ReadAsync(string path, CancellationToken cancellationToken)
    {
        await using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.Asynchronous);
        Snapshot? snapshot;
        try
        {
            snapshot = await JsonSerializer.DeserializeAsync(stream, JsonOptions.Create().Snapshot, cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"Malformed snapshot JSON at {exception.Path ?? "$"}.", exception);
        }

        if (snapshot is null)
        {
            throw new InvalidDataException("Snapshot is empty.");
        }
        if (snapshot.SchemaVersion <= 0)
        {
            throw new InvalidDataException("Snapshot schema version is missing or zero.");
        }
        if (snapshot.SchemaVersion > 1)
        {
            throw new InvalidDataException($"Snapshot schema version {snapshot.SchemaVersion} is newer than this tool supports.");
        }
        if (string.IsNullOrWhiteSpace(snapshot.ToolVersion) || string.IsNullOrWhiteSpace(snapshot.ConfigurationFingerprint) || snapshot.Source is null || snapshot.Scan is null)
        {
            throw new InvalidDataException("Snapshot is missing one or more required fields.");
        }
        return snapshot;
    }
}

using System.Text.Json;
using RedisKeyMap.Application.Abstractions;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Reporting.Json;

public sealed class SnapshotJsonWriter : ISnapshotWriter
{
    public async Task WriteAsync(Snapshot snapshot, string path, CancellationToken cancellationToken)
    {
        string fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        string temporary = Path.Combine(Path.GetDirectoryName(fullPath)!, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (FileStream stream = new(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, FileOptions.Asynchronous))
            {
                await JsonSerializer.SerializeAsync(stream, snapshot, JsonOptions.Create().Snapshot, cancellationToken).ConfigureAwait(false);
                await stream.WriteAsync("\n"u8.ToArray(), cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            File.Move(temporary, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }
}

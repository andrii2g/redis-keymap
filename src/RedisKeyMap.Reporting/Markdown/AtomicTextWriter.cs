using System.Text;

namespace RedisKeyMap.Reporting.Markdown;

internal static class AtomicTextWriter
{
    public static async Task WriteAsync(string path, string content, CancellationToken cancellationToken)
    {
        string fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        string temporary = Path.Combine(Path.GetDirectoryName(fullPath)!, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await File.WriteAllTextAsync(temporary, content.Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false), cancellationToken).ConfigureAwait(false);
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

using System.Runtime.CompilerServices;
using System.Text;
using RedisKeyMap.Application.Abstractions;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Application.Sources;

public sealed class TextFileKeySource(string path, bool preserveWhitespace = false, bool includeEmptyLines = false) : IKeySource
{
    public async IAsyncEnumerable<KeyObservation> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Input file not found: {Path.GetFileName(path)}", path);
        }

        UTF8Encoding encoding = new(false, true);
        await using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        using StreamReader reader = new(stream, encoding, true, 64 * 1024, false);
        while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is string line)
        {
            string value = preserveWhitespace ? line : line.Trim();
            if (value.Length == 0 && !includeEmptyLines)
            {
                continue;
            }

            byte[] bytes;
            try
            {
                bytes = encoding.GetBytes(value);
            }
            catch (EncoderFallbackException exception)
            {
                throw new InvalidDataException("Input contains invalid UTF-8 data.", exception);
            }
            yield return new(bytes, value, "file");
        }
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

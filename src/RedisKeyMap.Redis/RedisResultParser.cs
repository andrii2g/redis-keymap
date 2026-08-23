using System.Collections.Immutable;
using System.Globalization;
using StackExchange.Redis;

namespace RedisKeyMap.Redis;

public sealed record RedisScanPage(ulong Cursor, ImmutableArray<byte[]> Keys);

public static class RedisResultParser
{
    public static RedisScanPage Parse(RedisResult result)
    {
        RedisResult[] outer;
        try
        {
            outer = (RedisResult[]?)result ?? throw new InvalidDataException("SCAN response is null.");
        }
        catch (Exception exception) when (exception is not InvalidDataException)
        {
            throw new InvalidDataException("SCAN response must be a two-element array.", exception);
        }
        if (outer.Length != 2)
        {
            throw new InvalidDataException("SCAN response must be a two-element array.");
        }
        string? cursorText = (string?)outer[0];
        if (!ulong.TryParse(cursorText, NumberStyles.None, CultureInfo.InvariantCulture, out ulong cursor))
        {
            throw new InvalidDataException("SCAN response cursor is invalid.");
        }
        RedisResult[] values = (RedisResult[]?)outer[1] ?? throw new InvalidDataException("SCAN response keys must be an array.");
        ImmutableArray<byte[]>.Builder keys = ImmutableArray.CreateBuilder<byte[]>(values.Length);
        foreach (RedisResult value in values)
        {
            byte[] bytes = (byte[]?)value ?? throw new InvalidDataException("SCAN response contained a null key.");
            keys.Add(bytes);
        }
        return new(cursor, keys.ToImmutable());
    }
}

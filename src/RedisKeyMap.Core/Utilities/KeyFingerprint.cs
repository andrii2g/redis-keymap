using System.Buffers.Binary;
using System.Security.Cryptography;

namespace RedisKeyMap.Core.Utilities;

public static class KeyFingerprint
{
    public static ulong Compute64(ReadOnlySpan<byte> value)
    {
        Span<byte> digest = stackalloc byte[32];
        SHA256.HashData(value, digest);
        return BinaryPrimitives.ReadUInt64BigEndian(digest);
    }
}

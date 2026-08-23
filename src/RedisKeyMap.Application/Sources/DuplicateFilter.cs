using RedisKeyMap.Core.Models;
using RedisKeyMap.Core.Utilities;

namespace RedisKeyMap.Application.Sources;

public sealed class DuplicateFilter
{
    private readonly DuplicateHandling _mode;
    private readonly HashSet<ulong> _hashes = [];
    private readonly HashSet<byte[]> _exact = new(ByteComparer.Instance);

    public DuplicateFilter(DuplicateHandling mode) => _mode = mode;

    public long ObservedItems { get; private set; }
    public long AcceptedItems { get; private set; }
    public long DuplicateItems { get; private set; }

    public bool Accept(ReadOnlyMemory<byte> bytes)
    {
        ObservedItems++;
        bool accepted = _mode switch
        {
            DuplicateHandling.None => true,
            DuplicateHandling.Hash64 => _hashes.Add(KeyFingerprint.Compute64(bytes.Span)),
            DuplicateHandling.Exact => _exact.Add(bytes.ToArray()),
            _ => throw new ArgumentOutOfRangeException()
        };
        if (accepted)
        {
            AcceptedItems++;
        }
        else
        {
            DuplicateItems++;
        }

        return accepted;
    }

    private sealed class ByteComparer : IEqualityComparer<byte[]>
    {
        public static ByteComparer Instance { get; } = new();
        public bool Equals(byte[]? x, byte[]? y) => ReferenceEquals(x, y) || (x is not null && y is not null && x.AsSpan().SequenceEqual(y));
        public int GetHashCode(byte[] value) => unchecked((int)KeyFingerprint.Compute64(value));
    }
}

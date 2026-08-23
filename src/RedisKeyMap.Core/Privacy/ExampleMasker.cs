using System.Security.Cryptography;
using RedisKeyMap.Core.Models;
using RedisKeyMap.Core.Normalization;

namespace RedisKeyMap.Core.Privacy;

public sealed class ExampleMasker
{
    private readonly PrivacyOptions _options;
    private readonly string _delimiter;

    public ExampleMasker(PrivacyOptions options, string delimiter)
    {
        _options = options;
        _delimiter = delimiter;
    }

    public string? Mask(KeyObservation observation, KeyPattern pattern)
    {
        if (_options.ExampleMode == ExampleMode.None)
        {
            return null;
        }

        if (_options.ExampleMode == ExampleMode.Raw && observation.Text is not null)
        {
            return observation.Text;
        }

        if (pattern.ContainsBinaryData)
        {
            Span<byte> hash = stackalloc byte[32];
            SHA256.HashData(observation.RawBytes.Span, hash);
            return $"{SegmentMarkers.Binary}:sha256:{Convert.ToHexString(hash[..6]).ToLowerInvariant()}";
        }

        return string.Join(_delimiter, pattern.Segments.Select(segment =>
            segment.Kind == SegmentKind.Static && _options.MaskStaticSegments ? "{static}" : segment.Value));
    }
}

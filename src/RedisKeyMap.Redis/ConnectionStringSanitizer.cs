using System.Text.RegularExpressions;

namespace RedisKeyMap.Redis;

public static partial class ConnectionStringSanitizer
{
    public static string Sanitize(string message, string? exactSecret = null)
    {
        string result = UriPasswordRegex().Replace(message, "$1[REDACTED]$3");
        result = TokenPasswordRegex().Replace(result, "$1[REDACTED]");
        if (!string.IsNullOrEmpty(exactSecret))
        {
            result = result.Replace(exactSecret, "[REDACTED]", StringComparison.Ordinal);
        }
        return result;
    }

    [GeneratedRegex(@"(://[^:/\s]+:)([^@/\s]+)(@)", RegexOptions.CultureInvariant)]
    private static partial Regex UriPasswordRegex();

    [GeneratedRegex(@"(?i)(\b(?:password|pwd)\s*=\s*)[^,;\s]+", RegexOptions.CultureInvariant)]
    private static partial Regex TokenPasswordRegex();
}

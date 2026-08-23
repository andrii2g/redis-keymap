namespace RedisKeyMap.Reporting.Markdown;

public static class MarkdownEscaper
{
    public static string TableCell(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("|", "\\|", StringComparison.Ordinal)
        .Replace("\r", "\\r", StringComparison.Ordinal)
        .Replace("\n", "\\n", StringComparison.Ordinal);

    public static string Code(string value)
    {
        int fence = 1;
        int run = 0;
        foreach (char character in value)
        {
            run = character == '`' ? run + 1 : 0;
            fence = Math.Max(fence, run + 1);
        }
        string ticks = new('`', fence);
        return $"{ticks}{value}{ticks}";
    }
}

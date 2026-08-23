using System.Text;

namespace RedisKeyMap.Cli.Infrastructure;

public static class TerminalCapabilities
{
    public static bool SupportsAnsi(bool noColor) => !noColor && !Console.IsOutputRedirected;

    public static string StripAnsi(string value)
    {
        StringBuilder result = new(value.Length);
        for (int index = 0; index < value.Length; index++)
        {
            if (value[index] == (char)27 && index + 1 < value.Length && value[index + 1] == '[')
            {
                index += 2;
                while (index < value.Length && value[index] is < '@' or > '~')
                {
                    index++;
                }
                continue;
            }
            result.Append(value[index]);
        }
        return result.ToString();
    }
}

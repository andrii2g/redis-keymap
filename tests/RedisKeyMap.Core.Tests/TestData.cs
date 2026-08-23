using System.Text;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Core.Tests;

internal static class TestData
{
    public static KeyObservation Observation(string value) => new(Encoding.UTF8.GetBytes(value), value, "file");

    public static async IAsyncEnumerable<KeyObservation> Observations(params string[] values)
    {
        foreach (string value in values)
        {
            yield return Observation(value);
            await Task.Yield();
        }
    }
}

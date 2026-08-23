using System.Text;
using RedisKeyMap.Application.Sources;

namespace RedisKeyMap.Application.Tests;

public sealed class TextFileKeySourceTests
{
    [Fact]
    public async Task ReadAsync_WhenBomWhitespaceAndBlank_TrimsAndIgnoresBlank()
    {
        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, " user:1 \n\nproduct:2\n", new UTF8Encoding(true), TestContext.Current.CancellationToken);
            await using TextFileKeySource source = new(path);
            var values = await source.ReadAsync(TestContext.Current.CancellationToken).Select(item => item.Text).ToListAsync(TestContext.Current.CancellationToken);
            Assert.Equal(["user:1", "product:2"], values);
        }
        finally
        {
            File.Delete(path);
        }
    }
}

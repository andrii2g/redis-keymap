using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using RedisKeyMap.Application.Abstractions;
using RedisKeyMap.Core.Models;
using StackExchange.Redis;

namespace RedisKeyMap.Redis;

public sealed class RedisKeySource : IKeySource
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly ConnectionMultiplexer _connection;
    private readonly IServer _server;
    private readonly RedisScanOptions _options;

    private RedisKeySource(ConnectionMultiplexer connection, IServer server, RedisScanOptions options)
    {
        _connection = connection;
        _server = server;
        _options = options;
    }

    public string Endpoint => _server.EndPoint.ToString() ?? "redis";

    public static async Task<RedisKeySource> ConnectAsync(string connectionString, RedisScanOptions options, CancellationToken cancellationToken)
    {
        if (options.PageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Page size must be greater than zero.");
        }

        ConnectionMultiplexer connection = await RedisConnectionFactory.ConnectAsync(connectionString, cancellationToken).ConfigureAwait(false);
        try
        {
            IServer server = RedisServerCapabilityValidator.ResolveStandalone(connection);
            return new(connection, server, options);
        }
        catch
        {
            await connection.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async IAsyncEnumerable<KeyObservation> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ulong cursor = 0;
        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            object[] arguments =
            [
                cursor.ToString(CultureInfo.InvariantCulture),
                "MATCH",
                _options.Match,
                "COUNT",
                _options.PageSize
            ];
            RedisResult result;
            try
            {
                result = await _server.ExecuteAsync(_options.Database, "SCAN", arguments, CommandFlags.None)
                    .WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (RedisServerException exception) when (exception.Message.Contains("unknown command", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("Connected server does not support SCAN; Redis KeyMap refuses unsafe fallback.", exception);
            }
            RedisScanPage page = RedisResultParser.Parse(result);
            cursor = page.Cursor;
            foreach (byte[] bytes in page.Keys)
            {
                string? text = null;
                try
                {
                    text = StrictUtf8.GetString(bytes);
                }
                catch (DecoderFallbackException)
                {
                }
                yield return new(bytes, text, Endpoint);
            }
        }
        while (cursor != 0);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.CloseAsync().ConfigureAwait(false);
        await _connection.DisposeAsync().ConfigureAwait(false);
    }
}
